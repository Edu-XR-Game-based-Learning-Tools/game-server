using Grpc.Core;
using MagicOnion.Client;
using System;
using System.Threading.Tasks;
using VContainer.Unity;

namespace Shared.Network
{
    public class TimerHub : ITimerHubReceiver, IInitializable, IDisposable
    {
        public async ValueTask<int> ConnectAsync(ChannelBase grpcChannel, int value1, int value2)
        {
            var hubClient = await StreamingHubClient.ConnectAsync<ITimerHub, ITimerHubReceiver>(grpcChannel, this);

            return await hubClient.SumAsync(value1, value2);
        }

        public void Dispose()
        {
        }

        public void Initialize()
        {
        }

        public void OnTick(string message)
        {
        }
    }
}
