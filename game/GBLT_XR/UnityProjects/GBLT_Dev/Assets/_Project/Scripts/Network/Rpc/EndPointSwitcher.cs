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

        [Inject]
        private readonly ISubscriber<OnNetworkRetryExceedMaxRetriesSignal> _onNetworkRetryExceedMaxRetriesSubscriber;

        private DisposableBagBuilder _disposableBagBuilder;

        public EndPointSwitcher(
            NetworkSettings netWorkSettings)
        {
            _apiEndpoints = netWorkSettings.DefaultApiEndPoints;
        }

        public void UpdateEndPoints(string[] endpoints)
        {
            if (endpoints != null)
                _apiEndpoints = endpoints;
        }

        private void OnRetryFailed(OnNetworkRetryExceedMaxRetriesSignal e)
        {
            int previousIndex = _currentIndex;
            _currentIndex++;
            if (_currentIndex >= _apiEndpoints.Length)
                _currentIndex = 0;

            Debug.LogWarning($"Retry failed switch endpoint from {_apiEndpoints[previousIndex]} to {_apiEndpoints[_currentIndex]}");
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
    }
}
