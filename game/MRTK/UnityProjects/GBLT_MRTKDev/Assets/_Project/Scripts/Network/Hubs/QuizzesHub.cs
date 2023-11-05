using Core.Framework;
using Core.Network;
using Cysharp.Threading.Tasks;
using MessagePipe;
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

        public Task EndQuiz()
        {
            return _client.EndQuiz();
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

        public void OnJoin(QuizzesStatusResponse status, QuizzesUserData user)
        {
            _userDataController.ServerData.RoomStatus.InGameStatus = status;
            _virtualRoomPresenter.OnJoinQuizzes(user);
        }

        public void OnLeave(QuizzesStatusResponse status, QuizzesUserData user)
        {
            _userDataController.ServerData.RoomStatus.InGameStatus = status;
            _virtualRoomPresenter.OnLeaveQuizzes(user);
        }

        public void OnAnswer(AnswerData data)
        {
            _virtualRoomPresenter.OnAnswerQuizzes(data);
        }

        public void OnStart(QuizzesStatusResponse status)
        {
            _userDataController.ServerData.RoomStatus.InGameStatus = status;
            _virtualRoomPresenter.OnStartQuizzes(status);
        }

        public void OnDonePreview()
        {
            _virtualRoomPresenter.OnDonePreviewQuizzes();
        }

        public void OnEndQuestion()
        {
            _virtualRoomPresenter.OnEndQuestionQuizzes();
        }

        public void OnNextQuestion(QuizzesStatusResponse status)
        {
            _userDataController.ServerData.RoomStatus.InGameStatus = status;
            _virtualRoomPresenter.OnNextQuestionQuizzes(status);
        }

        public void OnEndQuiz()
        {
            _virtualRoomPresenter.OnEndQuizQuizzes();
        }

        #endregion Receivers of message from server.
    }
}
