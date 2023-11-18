using Core.Entity;
using Core.Service;
using MagicOnion.Server.Hubs;
using Microsoft.IdentityModel.Tokens;
using Shared;
using Shared.Extension;
using Shared.Network;

namespace RpcService.Hub
{
    public class QuizzesHub : GenericIdentityHub<IQuizzesHub, IQuizzesHubReceiver>, IQuizzesHub
    {
        private IGroup _room;

        private QuizzesUserData _self;
        private IInMemoryStorage<QuizzesUserData> _storage;
        private IQuizService _quizService;

        public QuizzesHub(IUserDataService userDataService, IJwtTokenValidator jwtTokenValidator, IRedisDataService redisDataService, IQuizService quizService) : base(userDataService, jwtTokenValidator, redisDataService)
        {
            _quizService = quizService;
        }

        protected override ValueTask OnDisconnected()
        {
            _ = LeaveAsync();
            return base.OnDisconnected();
        }

        public static string GetRoomCacheKey(string roomId)
        {
            return $"Quizzes/{roomId}";
        }

        private async Task<QuizzesStatusResponse> GetGameStatusFromCache(string roomId = null)
        {
            if (roomId == null && _room == null) return null;
            var status = await _redisDataService.GetCacheAsync<QuizzesStatusResponse>(GetRoomCacheKey(roomId ?? _room.GroupName));
            return status;
        }

        private async Task SaveGameStatusToCache(QuizzesStatusResponse status)
        {
            await _redisDataService.UpdateCacheAsync(GetRoomCacheKey(_room.GroupName), status);
        }

        #region Join Room

        private async Task<GeneralResponse> ValidateCreateRoom(JoinQuizzesData data)
        {
            var user = await GetUserIdentity();
            if (user == null)
                return new QuizzesStatusResponse { Success = false, Message = Defines.INVALID_SESSION };
            if (!data.UserData.IsHost)
                return new QuizzesStatusResponse { Success = false, Message = Defines.QUIZZES_NOT_TEACHER_CREATE };

            return null;
        }

        private static GeneralResponse ValidateJoinRoom(JoinQuizzesData data)
        {
            if (data == null || data.QuizzesStatus != QuizzesStatus.Pending)
                return new QuizzesStatusResponse { Success = false, Message = Defines.QUIZZES_UNAVAILABLE };

            return null;
        }

        private async Task<QuizzesStatusResponse> TryCreateRoom(JoinQuizzesData data)
        {
            if (!data.RoomId.IsNullOrEmpty()) return null;

            try
            {
                QuizzesStatusResponse validateMsg = (QuizzesStatusResponse)await ValidateCreateRoom(data);
                if (validateMsg != null) return validateMsg;

                data.RoomId = GenerateRoomId();
                _roomSet.Add(data.RoomId);

                _self.Index = -1;
                (_room, _storage) = await Group.AddAsync(data.RoomId, _self);

                QuizzesStatusResponse status = new() { Self = _self, AllInRoom = _storage.AllValues.ToArray(), Id = _room.GroupName, JoinQuizzesData = data };
                BroadcastExceptSelf(_room).OnJoin(status, _self);
                await SaveGameStatusToCache(status);
                return status;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                throw;
            }
        }

        public async Task<QuizzesStatusResponse> JoinAsync(JoinQuizzesData data)
        {
            _self = new()
            {
                UserData = data.UserData,
                QuizzesConnectionId = ConnectionId
            };
            var response = await TryCreateRoom(data);
            if (response != null) return response;

            try
            {
                QuizzesStatusResponse status = await GetGameStatusFromCache(data.RoomId);
                QuizzesStatusResponse validateMsg = (QuizzesStatusResponse)ValidateJoinRoom(status.JoinQuizzesData);
                if (validateMsg != null) return validateMsg;

                var allIndexesInRoom = status.AllInRoom.Select(user => user.Index).OrderBy(index => index);
                int index = -1;
                for (int idx = 0; idx < allIndexesInRoom.Count(); idx++)
                {
                    if (index != allIndexesInRoom.ElementAt(idx)) break;
                    index++;
                }
                _self.Index = index; // -1 for not count teacher, teacher seat: index = -1
                (_room, _storage) = await Group.AddAsync(data.RoomId, _self);

                status.Self = _self;
                status.AllInRoom = _storage.AllValues.ToArray();
                BroadcastExceptSelf(_room).OnJoin(status, _self);
                await SaveGameStatusToCache(status);
                return status;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                throw;
            }
        }

        #endregion Join Room

        private async Task<bool> RemoveContextIfNotExistRoomStatus(QuizzesStatusResponse status)
        {
            if (_room == null) return true;
            status ??= await GetGameStatusFromCache();
            if (status == null)
            {
                await _room.RemoveAsync(Context);
                return true;
            }
            return false;
        }

        public async Task LeaveAsync()
        {
            try
            {
                QuizzesStatusResponse status = null;
                if (_self.IsHost)
                {
                    _roomSet.Remove(_room.GroupName);
                    await _redisDataService.RemoveCacheAsync(GetRoomCacheKey(_room.GroupName));
                }
                else
                {
                    status = await GetGameStatusFromCache();
                    if (await RemoveContextIfNotExistRoomStatus(status)) return;
                    status.AllInRoom = _storage.AllValues.WhereNot(ele => ele.QuizzesConnectionId == ConnectionId).ToArray();
                    await SaveGameStatusToCache(status);
                }
                Broadcast(_room).OnLeave(status, _self);

                await _room.RemoveAsync(Context);
            }
            catch { }
        }

        #region Host API

        public async Task<QuizCollectionListDto> GetCollections()
        {
            var user = await GetUserIdentity();
            if (user == null) return null;
            QuizCollectionListDto quizCollectionListDto = await _quizService.GetQuizCollectionList(user.IdentityId);
            return quizCollectionListDto;
        }

        public async Task StartGame(QuizCollectionDto data)
        {
            // Debug Only: Duration
            foreach (var quiz in data.Quizzes) quiz.Duration = 10;

            QuizzesStatusResponse status = await GetGameStatusFromCache();
            status.JoinQuizzesData.CurrentQuestionIdx = 0;
            status.JoinQuizzesData.QuizzesStatus = QuizzesStatus.InProgress;
            status.QuizCollection = data;
            foreach (var student in status.AllInRoom)
                student.ResetPlayData();
            status.JoinQuizzesData.CurrentQuestionStartTime = DateTime.UtcNow;

            Broadcast(_room).OnStart(status);
            await SaveGameStatusToCache(status);
        }

        public Task DonePreview()
        {
            Broadcast(_room).OnDonePreview();
            return Task.CompletedTask;
        }

        private int _minScoreEachQuestion = 300;
        private int _maxScoreEachQuestion = 1000;

        private float CalculateFunction(int duration, int milliTaken)
        {
            // y = -diff/(duration-0.5)*(x-0.5f)+max
            var diff = _maxScoreEachQuestion - _minScoreEachQuestion;
            return (float)Math.Floor(Math.Clamp(-diff / (duration - 0.5f) * (milliTaken / 1000 - 0.5f) + _maxScoreEachQuestion, _minScoreEachQuestion, _maxScoreEachQuestion));
        }
        private void CalculateScoreThisQuestion(QuizzesStatusResponse status)
        {
            var students = status.Students;
            foreach (var student in students)
            {
                if (student.AnswerIdx == null) continue;

                QuizDto quiz = status.QuizCollection.Quizzes[status.JoinQuizzesData.CurrentQuestionIdx];
                if (quiz.CorrectIdx != student.AnswerIdx) continue;

                student.Score += CalculateFunction(quiz.Duration, student.AnswerMilliTimeFromStart);
            }

            var rankOrder = students
                .OrderByDescending(ele => ele.Score)
                .Select((ele, idx) => (ele, idx))
                .ToDictionary(ele => ele.ele.QuizzesConnectionId, ele => ele);
            foreach (var student in students)
                student.Rank = rankOrder[student.QuizzesConnectionId].idx + 1;
        }

        public async Task EndQuestion()
        {
            var status = await GetGameStatusFromCache();
            status.JoinQuizzesData.QuizzesStatus = QuizzesStatus.End;
            CalculateScoreThisQuestion(status);
            await SaveGameStatusToCache(status);

            Broadcast(_room).OnEndQuestion(status);
        }

        public async Task NextQuestion()
        {
            var status = await GetGameStatusFromCache();
            status.JoinQuizzesData.CurrentQuestionIdx++;
            status.JoinQuizzesData.CurrentQuestionStartTime = DateTime.UtcNow.AddSeconds(Defines.QUIZZES_PREVIEW_QUESTION_SECS);

            if (status.JoinQuizzesData.CurrentQuestionIdx == status.QuizCollection.Quizzes.Length)
                Broadcast(_room).OnEndQuiz(status);
            else
                Broadcast(_room).OnNextQuestion(status);
            await SaveGameStatusToCache(status);
        }

        public async Task EndSession()
        {
            var status = await GetGameStatusFromCache();
            status.ResetQuizzesSessionData();
            foreach (var student in status.AllInRoom)
                student.ResetPlayData();

            Broadcast(_room).OnEndSession();
            await SaveGameStatusToCache(status);
        }

        #endregion Host API

        public async Task Answer(AnswerData data)
        {
            data.UserData = _self;
            var status = await GetGameStatusFromCache();
            var host = status.AllInRoom.Where(ele => ele.IsHost).First();
            QuizzesUserData userData = status.AllInRoom.Where(ele => ele.QuizzesConnectionId == _self.QuizzesConnectionId).First();
            userData.AnswerIdx = data.AnswerIdx;
            DateTime answerTime = DateTime.UtcNow;
            TimeSpan diff = answerTime - status.JoinQuizzesData.CurrentQuestionStartTime;
            userData.AnswerMilliTimeFromStart = (int)Math.Floor(diff.TotalMilliseconds);
            await SaveGameStatusToCache(status);

            BroadcastTo(_room, (Guid)host.QuizzesConnectionId).OnAnswer(data);
        }
    }
}