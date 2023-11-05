using Core.Business;
using Core.EventSignal;
using Core.Framework;
using Core.Module;
using Core.Network;
using Cysharp.Threading.Tasks;
using MessagePipe;
using System;
using System.Threading.Tasks;
using UnityEngine;
using VContainer;

namespace Shared.Network
{
    public class ClassRoomHub : IClassRoomHubReceiver, IDisposable
    {
        private readonly IGRpcHubClient _gRpcHubClient;
        private readonly VirtualRoomPresenter _virtualRoomPresenter;
        private readonly QuizzesHub _quizzesHub;
        private readonly IUserDataController _userDataController;
        private readonly GameStore _gameStore;

        [Inject]
        private readonly ISubscriber<OnVirtualRoomTickSignal> _onVirtualRoomTickSubscriber;

        private DisposableBagBuilder _disposableBagBuilder;

        private HubStayAliveHelper _stayAliveHelper = null;

        private IClassRoomHub _client;

        [Inject]
        protected readonly IPublisher<ShowPopupSignal> _showPopupPublisher;

        public ClassRoomHub(
            IObjectResolver container)
        {
            _gRpcHubClient = container.Resolve<IGRpcHubClient>();
            _virtualRoomPresenter = container.Resolve<VirtualRoomPresenter>();
            _quizzesHub = container.Resolve<QuizzesHub>();
            _userDataController = container.Resolve<IUserDataController>();
            _gameStore = container.Resolve<GameStore>();
        }

        public void Init()
        {
            _disposableBagBuilder = DisposableBag.CreateBuilder();
            _onVirtualRoomTickSubscriber.Subscribe(OnVirtualRoomTickHandler).AddTo(_disposableBagBuilder);
        }

        public void Dispose()
        {
            _disposableBagBuilder?.Build().Dispose();
        }

        #region Methods send to server.

        public async Task<RoomStatusResponse> JoinAsync(JoinClassRoomData data, bool requiredAuthentication = false)
        {
            _client = await _gRpcHubClient.Subscribe<IClassRoomHub, IClassRoomHubReceiver>(this, requiredAuthentication: requiredAuthentication);
            _stayAliveHelper = new HubStayAliveHelper(() => _client.CmdToKeepAliveConnection().AsUniTask());
            RoomStatusResponse response;
            try
            {
                response = await _client.JoinAsync(data);
            }
            catch (Exception ex)
            {
                Debug.Log(ex);
                response = new RoomStatusResponse()
                {
                    Message = ex.Message,
                    Success = false,
                };
            }
            return response;
        }

        private void OnVirtualRoomTickHandler(OnVirtualRoomTickSignal signal)
        {
            _client.VirtualRoomTickSync(signal.TickData);
        }

        public async Task LeaveAsync()
        {
            _stayAliveHelper.Dispose();
            await _client.LeaveAsync();
        }

        public async Task InviteToGame(QuizzesStatusResponse quizzesResponse)
        {
            await _client.InviteToGame(new InviteToGameData()
            {
                RoomId = quizzesResponse.Id,
            });
        }

        public async Task UpdateAvatar(string name, string modelPath, string avatarPath)
        {
            await _client.UpdateAvatar(name, modelPath, avatarPath);
        }

        public async Task Tick(string message)
        {
            _client = await _gRpcHubClient.Subscribe<IClassRoomHub, IClassRoomHubReceiver>(this);
            await _client.Tick(message);
        }

        #endregion Methods send to server.

        // dispose client-connection before channel.ShutDownAsync is important!
        public async Task DisposeAsync()
        {
            _stayAliveHelper.Dispose();
            await _client.DisposeAsync();
        }

        // You can watch connection state, use this for retry etc.
        public async Task WaitForDisconnect()
        {
            await _client.WaitForDisconnect();
        }

        #region Receivers of message from server.

        public void OnJoin(RoomStatusResponse status, PublicUserData user)
        {
            Debug.Log("Join User:" + user.Name);

            _userDataController.ServerData.RoomStatus.RoomStatus = status;
            _virtualRoomPresenter.OnJoin(user);
        }

        public void OnLeave(RoomStatusResponse status, PublicUserData user)
        {
            Debug.Log("Leave User:" + user.Name);

            _virtualRoomPresenter.OnLeave(user);
            _userDataController.ServerData.RoomStatus.RoomStatus = _userDataController.ServerData.RoomStatus.RoomStatus.Self.Index == user.Index ? null : status;
        }

        public void OnRoomTick(VirtualRoomTickResponse response)
        {
            _virtualRoomPresenter.OnRoomTick(response);
        }

        public void OnTick(string message)
        {
            Debug.Log(message);
        }

        public void OnInviteToGame(InviteToGameData data, PublicUserData inviter)
        {
            _showPopupPublisher.Publish(new ShowPopupSignal(title: $"Host invite you to {data.ToolType} tool, Do you accept it?", yesContent: "Yes", noContent: "No", yesAction: async (value1, value2) =>
            {
                QuizzesStatusResponse response = await _quizzesHub.JoinAsync(data.JoinQuizzesData);
                _userDataController.ServerData.RoomStatus.InGameStatus = response;

                _gameStore.RemoveCurrentModel();
                await _gameStore.GetOrCreateModule<QuizzesRoomStatus, QuizzesRoomStatusModel>(
                    "", ViewName.Unity, ModuleName.QuizzesRoomStatus);
            }, noAction: (_, _) => { }));
        }

        public void OnUpdateAvatar(PublicUserData user)
        {
            _virtualRoomPresenter.OnUpdateAvatar(user);
        }

        #endregion Receivers of message from server.
    }
}
