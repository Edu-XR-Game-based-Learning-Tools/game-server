using Core.EventSignal;
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
    public class ClassRoomHub : IClassRoomHubReceiver, IInitializable, IDisposable
    {
        private readonly IGRpcHubClient _gRpcHubClient;
        private readonly VirtualRoomPresenter _virtualRoomPresenter;

        [Inject]
        private readonly ISubscriber<OnVirtualRoomTickSignal> _onVirtualRoomTickSubscriber;

        private DisposableBagBuilder _disposableBagBuilder;

        private HubStayAliveHelper _stayAliveHelper = null;

        private IClassRoomHub _client;

        public ClassRoomHub(
            IObjectResolver container)
        {
            _gRpcHubClient = container.Resolve<IGRpcHubClient>();
            _virtualRoomPresenter = container.Resolve<VirtualRoomPresenter>();
        }

        public void Initialize()
        {
            _disposableBagBuilder = DisposableBag.CreateBuilder();
            _onVirtualRoomTickSubscriber.Subscribe(OnVirtualRoomTick).AddTo(_disposableBagBuilder);
        }

        public void Dispose()
        {
            _disposableBagBuilder?.Build().Dispose();
        }

        #region Methods send to server.

        public async Task<RoomStatusResponse> JoinAsync(JoinClassRoomData data)
        {
            _client = await _gRpcHubClient.Subscribe<IClassRoomHub, IClassRoomHubReceiver>(this, requiredAuthentication: false);
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

        private void OnVirtualRoomTick(OnVirtualRoomTickSignal signal)
        {
            _client.VirtualRoomTickSync(signal.TickData);
        }

        public Task LeaveAsync()
        {
            _stayAliveHelper.Dispose();
            return _client.LeaveAsync();
        }

        #endregion Methods send to server.

        // dispose client-connection before channel.ShutDownAsync is important!
        public Task DisposeAsync()
        {
            _stayAliveHelper.Dispose();
            _virtualRoomPresenter.Clean();
            return _client.DisposeAsync();
        }

        // You can watch connection state, use this for retry etc.
        public Task WaitForDisconnect()
        {
            return _client.WaitForDisconnect();
        }

        #region Receivers of message from server.

        public void OnJoin(PublicUserData user)
        {
            Debug.Log("Join User:" + user.Name);

            _virtualRoomPresenter.OnJoin(user);
        }

        public void OnLeave(PublicUserData user)
        {
            Debug.Log("Leave User:" + user.Name);

            _virtualRoomPresenter.OnLeave(user);
        }

        public void OnRoomTick(VirtualRoomTickResponse response)
        {
            _virtualRoomPresenter.OnRoomTick(response);
        }

        public void OnTick(string message)
        {
            Debug.Log(message);
        }

        #endregion Receivers of message from server.
    }
}
