using Core.Business;
using Core.EventSignal;
using Cysharp.Threading.Tasks;
using MessagePipe;
using Newtonsoft.Json;
using Proyecto26;
using Shared.Network;
using System;
using VContainer;

namespace Core.Network
{
    public class DataRestController : IDataServiceController, IDisposable
    {
        private readonly EndPointSwitcher _endPointSwitcher;
        private readonly UserAuthentication _userAuthentication;
        private readonly IUserDataController _userDataController;

        [Inject]
        private readonly IPublisher<UserDataCachedSignal> _userDataCachedPublisher;

        public bool IsExpired => _userAuthentication == null || _userAuthentication.IsExpired;

        private static long _nextSyncTime = 0;
        private int _minDataSyncIntervalInseconds = 5;

        public DataRestController(
            EndPointSwitcher endPointSwitcher,
            UserAuthentication userAuthenticationData,
            IUserDataController userDataController)
        {
            _endPointSwitcher = endPointSwitcher;
            _userAuthentication = userAuthenticationData;
            _userDataController = userDataController;
        }

        public void Dispose()
        { }

        public async UniTask CacheUserDatas()
        {
            if (_nextSyncTime != 0 && DateTime.UtcNow.Ticks < _nextSyncTime)
                return;

            bool isDoneRequest = false;
            RestClient.Get($"{_endPointSwitcher.ApiEndPoint}/api/user/syncUserData").Then(response =>
            {
                _userDataController.ServerData.UserData = JsonConvert.DeserializeObject<UserData>(response.Text);
                isDoneRequest = true;
            });
            await UniTask.WaitUntil(() => isDoneRequest);

            _nextSyncTime = DateTime.UtcNow.AddSeconds(_minDataSyncIntervalInseconds).Ticks;

            _userDataCachedPublisher.Publish(new UserDataCachedSignal());
        }

        public async UniTask<byte[]> LoadDefinitions()
        {
            bool isDoneRequest = false;
            byte[] definitions = null;
            RestClient.Get($"{_endPointSwitcher.ApiEndPoint}/api/generic/getDefinitions")
            .Then(response =>
            {
                definitions = JsonConvert.DeserializeObject<byte[]>(response.Text);
                isDoneRequest = true;
            });
            await UniTask.WaitUntil(() => isDoneRequest);

            UnityEngine.Debug.Log($"LoadDefinitions: {definitions}");
            return definitions;
        }

        public async UniTask<DateTime> GetServerTime()
        {
            bool isDoneRequest = false;
            DateTime serverTime = DateTime.Now;
            RestClient.Get($"{_endPointSwitcher.ApiEndPoint}/api/generic/getServerTime").Then(response =>
            {
                serverTime = JsonConvert.DeserializeObject<DateTime>(response.Text);
                isDoneRequest = true;
            });
            await UniTask.WaitUntil(() => isDoneRequest);

            UnityEngine.Debug.Log($"GetServerTime: {serverTime}");
            return serverTime;
        }
    }
}
