using MagicOnion;
using System;
using System.Threading.Tasks;

namespace Shared.Network
{
    public interface ITimerHub : IStreamingHub<ITimerHub, ITimerHubReceiver>
    {
        Task SetAsync(TimeSpan interval);
    }

    public interface ITimerHubReceiver
    {
        void OnTick(string message);
    }
}