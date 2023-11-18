using Core.Framework;
using Core.Network;
using Cysharp.Threading.Tasks;
using MessagePipe;
using Models;
using Shared.Extension;
using System;
using System.Threading.Tasks;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Shared.Network
{
    public class QuizzesHub : IQuizzesHubReceiver, IInitializable, IDisposable
    {
        private readonly IGRpcHubClient _gRpcHubClient;
        private readonly VirtualRoomPresenter _virtualRoomPresenter;
        private readonly IUserDataController _userDataController;

        private DisposableBagBuilder _disposableBagBuilder;

        private HubStayAliveHelper _stayAliveHelper = null;

        private IQuizzesHub _client;

        public QuizzesHub(
            IObjectResolver container)
        {
            _gRpcHubClient = container.Resolve<IGRpcHubClient>();
            _virtualRoomPresenter = container.Resolve<VirtualRoomPresenter>();
            _userDataController = container.Resolve<IUserDataController>();
        }

        public void Initialize()
        {
            _disposableBagBuilder = DisposableBag.CreateBuilder();
        }

        public void Dispose()
        {
            _disposableBagBuilder?.Build().Dispose();
        }

        #region Methods send to server.

        public async Task<QuizzesStatusResponse> JoinAsync(JoinQuizzesData data, bool requiredAuthentication = false)
        {
            data.UserData = _userDataController.ServerData.RoomStatus.RoomStatus.Self;
            _client = await _gRpcHubClient.Subscribe<IQuizzesHub, IQuizzesHubReceiver>(this, requiredAuthentication: requiredAuthentication);
            _stayAliveHelper = new HubStayAliveHelper(() => _client.CmdToKeepAliveConnection().AsUniTask());
            QuizzesStatusResponse response;
            try
            {
                response = await _client.JoinAsync(data);
            }
            catch (Exception ex)
            {
                Debug.Log(ex);
                response = new QuizzesStatusResponse()
                {
                    Message = ex.Message,
                    Success = false,
                };
            }
            return response;
        }

        public Task LeaveAsync()
        {
            _stayAliveHelper.Dispose();
            return _client.LeaveAsync();
        }

        // Host
        public Task<QuizCollectionListDto> GetCollections()
        {
            return _client.GetCollections();
        }

        public Task StartGame(QuizCollectionDto collection)
        {
            return _client.StartGame(collection);
        }

        public Task DonePreview()
        {
            return _client.DonePreview();
        }

        public Task EndQuestion()
        {
            return _client.EndQuestion();
        }

        public Task NextQuestion()
        {
            return _client.NextQuestion();
        }

        public Task EndSession()
        {
            return _client.EndSession();
        }

        // Player
        public Task Answer(AnswerData data)
        {
            return _client.Answer(data);
        }

        #endregion Methods send to server.

        // dispose client-connection before channel.ShutDownAsync is important!
        public Task DisposeAsync()
        {
            _stayAliveHelper.Dispose();
            return _client.DisposeAsync();
        }

        // You can watch connection state, use this for retry etc.
        public Task WaitForDisconnect()
        {
            return _client.WaitForDisconnect();
        }

        #region Receivers of message from server.

        private void UpdateStatusExceptSelf(QuizzesStatusResponse status)
        {
            QuizzesUserData self = null;
            if (_userDataController.ServerData.RoomStatus.InGameStatus != null)
                self = _userDataController.ServerData.RoomStatus.InGameStatus.Self;

            _userDataController.ServerData.RoomStatus.InGameStatus = status;
            if (_userDataController.ServerData.IsInGame && status.Self != null)
                _userDataController.ServerData.RoomStatus.InGameStatus.Self = self.QuizzesConnectionId == status.Self.QuizzesConnectionId ? status.Self : self;
        }

        public void OnJoin(QuizzesStatusResponse status, QuizzesUserData user)
        {
            UpdateStatusExceptSelf(status);
            _virtualRoomPresenter.OnJoinQuizzes(user);
        }

        public void OnLeave(QuizzesStatusResponse status, QuizzesUserData user)
        {
            if (user.IsHost && user.QuizzesConnectionId != _userDataController.ServerData.RoomStatus.InGameStatus.Self.QuizzesConnectionId)
            {
                _ = LeaveAsync();
                return;
            }
            UpdateStatusExceptSelf(status);
            _virtualRoomPresenter.OnLeaveQuizzes(user);
        }

        #region Only Host

        public void OnAnswer(AnswerData data)
        {
            _virtualRoomPresenter.OnAnswerQuizzes(data);
        }

        #endregion Only Host

        public void OnStart(QuizzesStatusResponse status)
        {
            UpdateStatusExceptSelf(status);
            if (_userDataController.ServerData.IsInGame) _userDataController.ServerData.RoomStatus.InGameStatus.RefreshSelfDataWithList();
            _virtualRoomPresenter.OnStartQuizzes();
        }

        public void OnDonePreview()
        {
            _virtualRoomPresenter.OnDonePreviewQuizzes();
        }

        public void OnEndQuestion(QuizzesStatusResponse status)
        {
            UpdateStatusExceptSelf(status);
            if (_userDataController.ServerData.IsInGame) _userDataController.ServerData.RoomStatus.InGameStatus.RefreshSelfDataWithList();
            _virtualRoomPresenter.OnEndQuestionQuizzes();
        }

        public void OnNextQuestion(QuizzesStatusResponse status)
        {
            UpdateStatusExceptSelf(status);
            _virtualRoomPresenter.OnNextQuestionQuizzes();
        }

        public void OnEndQuiz(QuizzesStatusResponse status)
        {
            UpdateStatusExceptSelf(status);
            if (_userDataController.ServerData.IsInGame) _userDataController.ServerData.RoomStatus.InGameStatus.RefreshSelfDataWithList();
            _virtualRoomPresenter.OnEndQuizQuizzes();
        }

        public void OnEndSession()
        {
            if (_userDataController.ServerData.IsInGame) _userDataController.ServerData.RoomStatus.InGameStatus.RefreshSelfDataWithList();
            _virtualRoomPresenter.OnEndSessionQuizzes();
        }

        #endregion Receivers of message from server.
    }
}
