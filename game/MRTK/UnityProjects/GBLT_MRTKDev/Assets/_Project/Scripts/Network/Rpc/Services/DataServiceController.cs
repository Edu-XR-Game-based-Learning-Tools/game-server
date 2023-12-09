using Core.Business;
using Core.EventSignal;
using Cysharp.Threading.Tasks;
using MagicOnion.Client;
using MessagePipe;
using Shared.Network;
using System;
using VContainer;

namespace Core.Network
{
    public class DataServiceController : IDataServiceController, IDisposable
    {
        private readonly IGRpcServiceClient _gRpcServiceClient;
        private readonly UserAuthentication _userAuthentication;
        private readonly IUserDataController _userDataController;

        [Inject]
        private readonly IPublisher<UserDataCachedSignal> _userDataCachedPublisher;

        private IClientFilter[] _authenClientFilter;
        private IClientFilter[] _unAuthenClientFilter;

        public bool IsExpired => _userAuthentication == null || _userAuthentication.IsExpired;

        private static long _nextSyncTime = 0;
        private int _minDataSyncIntervalInseconds = 5;

        public DataServiceController(
            IGRpcServiceClient gRpcServiceClient,
            UserAuthentication userAuthenticationData,
            GRpcAuthenticationFilter gRpcAuthenticationFilter,
            GRpcRetryHandlerFilter grpcErrorHandlerFilter)
        {
            _gRpcServiceClient = gRpcServiceClient;
            _userAuthentication = userAuthenticationData;
            _authenClientFilter = new IClientFilter[] { gRpcAuthenticationFilter, grpcErrorHandlerFilter };
            _unAuthenClientFilter = new[] { grpcErrorHandlerFilter };
        }

        public void Dispose()
        { }

        public async UniTask CacheUserDatas()
        {
            if (_nextSyncTime != 0 && DateTime.UtcNow.Ticks < _nextSyncTime)
                return;
            _userDataController.ServerData.UserData = await _gRpcServiceClient.CreateServiceWithFilter<IRpcUserService>(_authenClientFilter).SyncUserData();

            _nextSyncTime = DateTime.UtcNow.AddSeconds(_minDataSyncIntervalInseconds).Ticks;

            _userDataCachedPublisher.Publish(new UserDataCachedSignal());
        }

        public async UniTask<byte[]> LoadDefinitions()
        {
            var client = _gRpcServiceClient.CreateServiceWithFilter<IGenericService>(_unAuthenClientFilter);
            UnityEngine.Debug.Log($"_gRpcServiceClient.CreateServiceWithFilter: {client}");
            byte[] definitions = await client.GetDefinitions();
            UnityEngine.Debug.Log($"LoadDefinitions: {definitions}");
            return definitions;
        }

        public async UniTask<DateTime> GetServerTime()
        {
            var serverTime = await _gRpcServiceClient.CreateServiceWithFilter<IGenericService>(_unAuthenClientFilter).GetServerTime();
            return serverTime;
        }

        public UniTask<EnvironmentGenericConfig> GetGenericConfig()
        {
            throw new NotImplementedException();
        }
    }
}
