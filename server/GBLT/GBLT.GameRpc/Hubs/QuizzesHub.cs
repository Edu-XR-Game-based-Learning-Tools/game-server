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

        private async Task<QuizzesStatusResponse> GetGameStatus(string roomId = null)
        {
            var quizzesStatus = await _redisDataService.GetCacheAsync<QuizzesStatusResponse>(GetRoomCacheKey(roomId ?? _room.GroupName));
            return quizzesStatus;
        }

        private async Task SaveGameStatus(QuizzesStatusResponse gStatus)
        {
            await _redisDataService.UpdateCacheAsync(GetRoomCacheKey(_room.GroupName), gStatus);
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
            if (data.QuizzesStatus != QuizzesStatus.Pending)
                return new QuizzesStatusResponse { Success = false, Message = Defines.QUIZZES_UNAVAILABLE };

            return null;
        }

        public async Task<QuizzesStatusResponse> JoinAsync(JoinQuizzesData data)
        {
            _self = new()
            {
                UserData = data.UserData,
                QuizzesConnectionId = ConnectionId
            };
            QuizzesStatusResponse validateMsg;
            bool isHost = false;
            if (data.RoomId.IsNullOrEmpty())
            {
                validateMsg = (QuizzesStatusResponse)await ValidateCreateRoom(data);
                if (validateMsg != null) return validateMsg;

                data.RoomId = GenerateRoomId();
                _roomSet.Add(data.RoomId);
                isHost = true;
            }

            QuizzesStatusResponse quizzesStatus = await GetGameStatus(data.RoomId);
            if (quizzesStatus != null)
            {
                validateMsg = (QuizzesStatusResponse)ValidateJoinRoom(quizzesStatus.JoinQuizzesData);
                if (validateMsg != null) return validateMsg;
            }

            (_room, _storage) = await Group.AddAsync(data.RoomId, _self);
            _self.Index = isHost ? -1 : _storage.AllValues.Count - 1; // -1 for not count teacher, teacher seat: index = -1

            QuizzesStatusResponse newGameData = new() { Self = _self, AllInRoom = _storage.AllValues.ToArray(), Id = _room.GroupName, JoinQuizzesData = data };
            await SaveGameStatus(newGameData);

            BroadcastExceptSelf(_room).OnJoin(newGameData, _self);
            return await GetGameStatus();
        }

        #endregion Join Room

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
                    var gStatus = await GetGameStatus();
                    gStatus.AllInRoom.RemoveWhere(ele => ele.QuizzesConnectionId == ConnectionId);
                    await SaveGameStatus(gStatus);
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

            var gStatus = await GetGameStatus();
            gStatus.JoinQuizzesData.CurrentQuestionIdx = 0;
            gStatus.JoinQuizzesData.QuizzesStatus = QuizzesStatus.InProgress;
            gStatus.QuizCollection = data;
            await SaveGameStatus(gStatus);

            Broadcast(_room).OnStart(gStatus);
        }

        public Task DonePreview()
        {
            Broadcast(_room).OnDonePreview();
            return Task.CompletedTask;
        }

        private int _minScoreEachQuestion = 300;
        private int _maxScoreEachQuestion = 1000;

        private void CalculateScoreThisQuestion(QuizzesStatusResponse gStatus)
        {
            var students = gStatus.Students;
            foreach (var student in students)
            {
                if (student.AnswerIdx == null) continue;

                QuizDto quiz = gStatus.QuizCollection.Quizzes[gStatus.JoinQuizzesData.CurrentQuestionIdx];
                if (quiz.CorrectIdx != student.AnswerIdx) continue;

                student.Score += Math.Clamp(student.AnswerMilliTimeFromStart / (quiz.Duration * 1000f), _minScoreEachQuestion, _maxScoreEachQuestion);
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
            var gStatus = await GetGameStatus();
            gStatus.JoinQuizzesData.QuizzesStatus = QuizzesStatus.End;
            CalculateScoreThisQuestion(gStatus);
            await SaveGameStatus(gStatus);

            Broadcast(_room).OnEndQuestion(gStatus);
        }

        public async Task NextQuestion()
        {
            var gStatus = await GetGameStatus();
            gStatus.JoinQuizzesData.CurrentQuestionIdx++;
            await SaveGameStatus(gStatus);

            if (gStatus.JoinQuizzesData.CurrentQuestionIdx == gStatus.QuizCollection.Quizzes.Length)
                Broadcast(_room).OnEndQuiz(gStatus);
            else
                Broadcast(_room).OnNextQuestion(gStatus);
        }

        public async Task EndSession()
        {
            var gStatus = await GetGameStatus();
            gStatus.JoinQuizzesData.QuizzesStatus = QuizzesStatus.Pending;
            await SaveGameStatus(gStatus);

            Broadcast(_room).OnEndSession();
        }

        #endregion Host API

        public async Task Answer(AnswerData data)
        {
            data.UserData = _self;
            var gStatus = await GetGameStatus();
            var host = gStatus.AllInRoom.Where(ele => ele.IsHost).First();
            QuizzesUserData userData = gStatus.AllInRoom.Where(ele => ele.QuizzesConnectionId == _self.QuizzesConnectionId).First();
            userData.AnswerIdx = data.AnswerIdx;
            await SaveGameStatus(gStatus);

            BroadcastTo(_room, (Guid)host.QuizzesConnectionId).OnAnswer(data);
        }
    }
}