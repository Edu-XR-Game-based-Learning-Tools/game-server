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

        private QuizzesStatusResponse _gStatus;
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

        #region Join Room

        private async Task<GeneralResponse> ValidateCreateRoom(JoinQuizzesData _)
        {
            var user = await GetUserIdentity();
            if (user == null)
                return new QuizzesStatusResponse { Success = false, Message = Defines.INVALID_SESSION };

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

                string roomId = GenerateRoomId();
                _roomSet.Add(roomId);
                await _redisDataService.UpdateCacheAsync(GetRoomCacheKey(roomId), data);
                isHost = true;
            }

            (_room, _storage) = await Group.AddAsync(data.RoomId, _self);
            _self.Index = isHost ? -1 : _storage.AllValues.Count - 1; // -1 for not count teacher, teacher seat: index = -1

            QuizzesStatusResponse status = new() { Self = _self, AllInRoom = _storage.AllValues.ToArray(), Id = _room.GroupName, JoinQuizzesData = data };
            if (_gStatus.JoinQuizzesData.QuizzesStatus == QuizzesStatus.Pending) _gStatus = status;

            BroadcastExceptSelf(_room).OnJoin(status, _self);
            return status;
        }

        #endregion Join Room

        public async Task LeaveAsync()
        {
            if (_self.IsHost)
            {
                _roomSet.Remove(_room.GroupName);
                await _redisDataService.RemoveCacheAsync(GetRoomCacheKey(_room.GroupName));
            }
            await _room.RemoveAsync(Context);

            QuizzesStatusResponse status = null;
            if (!_self.IsHost)
                status = new() { Self = _self, AllInRoom = _storage.AllValues.ToArray(), Id = _room.GroupName };
            Broadcast(_room).OnLeave(status, _self);
        }

        #region Host API

        public async Task<QuizCollectionListDto> GetCollections()
        {
            var user = await GetUserIdentity();
            QuizCollectionListDto quizCollectionListDto = await _quizService.GetQuizCollectionList(user.IdentityId);
            return quizCollectionListDto;
        }

        public Task StartGame(QuizCollectionDto data)
        {
            _gStatus.JoinQuizzesData.CurrentQuestionIdx = 0;
            _gStatus.JoinQuizzesData.QuizzesStatus = QuizzesStatus.InProgress;
            _gStatus.QuizCollection = data;
            BroadcastExceptSelf(_room).OnStart(_gStatus);
            return Task.CompletedTask;
        }

        public Task DonePreview()
        {
            BroadcastExceptSelf(_room).OnDonePreview();
            return Task.CompletedTask;
        }

        public Task EndQuestion()
        {
            BroadcastExceptSelf(_room).OnEndQuestion();
            return Task.CompletedTask;
        }

        public Task NextQuestion()
        {
            _gStatus.JoinQuizzesData.CurrentQuestionIdx++;
            BroadcastExceptSelf(_room).OnNextQuestion(_gStatus);
            return Task.CompletedTask;
        }

        public Task EndQuiz()
        {
            BroadcastExceptSelf(_room).OnEndQuiz();
            return Task.CompletedTask;
        }

        #endregion Host API

        public Task Answer(AnswerData data)
        {
            var host = _storage.AllValues.Where(ele => ele.IsHost).First();
            data.UserData = _self;
            data.UserData.CorrectIdx = data.AnswerIdx;
            BroadcastTo(_room, (Guid)host.QuizzesConnectionId).OnAnswer(data);
            return Task.CompletedTask;
        }
    }
}