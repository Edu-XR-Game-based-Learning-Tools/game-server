using Core.EventSignal;
using MessagePipe;
using System;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Core.Network
{
    public class EndPointSwitcher : IInitializable, IDisposable
    {
        private int _currentIndex = 0;
        private string[] _apiEndpoints;

        private int _currentHubIndex = 0;
        private string[] _hubEndpoints;

        [Inject]
        private readonly ISubscriber<OnNetworkRetryExceedMaxRetriesSignal> _onNetworkRetryExceedMaxRetriesSubscriber;

        private DisposableBagBuilder _disposableBagBuilder;

        public EndPointSwitcher(
            NetworkSettings netWorkSettings)
        {
            _apiEndpoints = netWorkSettings.DefaultApiEndPoints[(int)netWorkSettings.HostType].Array;
            _hubEndpoints = netWorkSettings.DefaultHubEndPoints[(int)netWorkSettings.HostType].Array;
        }

        private void OnRetryFailed(OnNetworkRetryExceedMaxRetriesSignal e)
        {
            int previousIndex = _currentHubIndex;
            _currentHubIndex++;
            if (_currentHubIndex >= HubEndpoint.Length)
                _currentHubIndex = 0;

            Debug.LogWarning($"Retry failed switch endpoint from {HubEndpoint[previousIndex]} to {HubEndpoint[_currentHubIndex]}");
        }

        public void Initialize()
        {
            _disposableBagBuilder = DisposableBag.CreateBuilder();
            _onNetworkRetryExceedMaxRetriesSubscriber.Subscribe(OnRetryFailed).AddTo(_disposableBagBuilder);
        }

        public void Dispose()
        {
            _disposableBagBuilder?.Build().Dispose();
        }

        public string ApiEndPoint => _apiEndpoints[_currentIndex];
        public string HubEndpoint => _hubEndpoints[_currentHubIndex];
    }
}
