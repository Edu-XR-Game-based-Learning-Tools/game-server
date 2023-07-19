using Cysharp.Threading.Tasks;
using System;
using System.Threading;

namespace Shared.Network
{
    public class HubStayAliveHelper : IDisposable
    {
        private readonly Action _keepAliveAction = null;
        private const int KEEP_ALIVE_INTERVAL = 30;
        private int _syncingAliveSecond = 0;
        private readonly CancellationTokenSource _cancelToken;

        public HubStayAliveHelper(Action keepAliveAction)
        {
            _cancelToken = new CancellationTokenSource();
            KeepAliveConnection();
            _keepAliveAction = keepAliveAction;
        }

        private async void KeepAliveConnection()
        {
            _syncingAliveSecond = 0;
            while (true)
            {
                var isCanceled = await UniTask.Delay(10000, cancellationToken: _cancelToken.Token).SuppressCancellationThrow();
                _syncingAliveSecond += 10;
                if (_syncingAliveSecond >= KEEP_ALIVE_INTERVAL && !isCanceled)
                {
                    _syncingAliveSecond = 0;
                    _keepAliveAction?.Invoke();
                }
                if (isCanceled)
                    break;
            }
        }

        public void Dispose()
        {
            _cancelToken.Cancel();
        }
    }
}
