using Grpc.Core;
using MagicOnion;
using MagicOnion.Server.Hubs;
using Microsoft.AspNetCore.Authorization;
using Shared.Network;
using System.Security.Claims;

namespace RpcService.Hub
{
    public class TimerHub : StreamingHubBase<ITimerHub, ITimerHubReceiver>, ITimerHub
    {
        private Task _timerLoopTask;
        private readonly CancellationTokenSource _cancellationTokenSource = new();
        private TimeSpan _interval = TimeSpan.FromSeconds(1);
        private IGroup _group;

        public async Task SetAsync(TimeSpan interval)
        {
            if (_timerLoopTask != null) throw new InvalidOperationException("The timer has been already started.");

            _group = await this.Group.AddAsync(ConnectionId.ToString());
            _interval = interval;
            _timerLoopTask = Task.Run(async () =>
            {
                while (!_cancellationTokenSource.IsCancellationRequested)
                {
                    await Task.Delay(_interval, _cancellationTokenSource.Token);

                    var userPrincipal = Context.CallContext.GetHttpContext().User;
                    BroadcastToSelf(_group).OnTick($"UserId={userPrincipal.Claims.First(x => x.Type == ClaimTypes.NameIdentifier).Value}; Name={userPrincipal.Identity?.Name}");
                }
            });
        }

        // `UnaryResult<T>` allows the method to be treated as `async` method.
        public async Task<int> SumAsync(int x, int y)
        {
            Console.WriteLine($"Received:{x}, {y} {DateTime.Now}");
            await Task.Delay(1000);
            Console.WriteLine($"Task.Delay(1000) {DateTime.Now}");
            return x + y;
        }

        protected override ValueTask OnDisconnected()
        {
            _cancellationTokenSource.Cancel();
            return base.OnDisconnected();
        }
    }
}