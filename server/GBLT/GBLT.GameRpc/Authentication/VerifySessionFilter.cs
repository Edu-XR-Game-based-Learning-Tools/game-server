using Core.Service;
using Grpc.Core;
using MagicOnion.Server;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace RpcService.Authentication
{
    public class VerifySessionFilter : MagicOnionFilterAttribute
    {
        private readonly ILogger _logger;
        private readonly IServiceProvider _serviceProvider;

        public VerifySessionFilter(
            IServiceProvider serviceProvider,
            ILogger<VerifySessionFilter> logger)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
        }

        public override async ValueTask Invoke(ServiceContext context, Func<ServiceContext, ValueTask> next)
        {
            var identity = context.CallContext.GetHttpContext().User.Identity;
            if (identity.IsAuthenticated)
            {
                int userId = ((CustomJwtAuthUserIdentity)identity).Id;
                string sessionId = ((CustomJwtAuthUserIdentity)identity).SessionId;
                _logger.LogDebug("VerifySessionFilter {context.ServiceType}: {identity.IsAuthenticated}\n- Session {sessionId}",
                    context.ServiceType, identity.IsAuthenticated, sessionId);

                using var scope = _serviceProvider.CreateScope();
                IUserAccountDataService userDataService = scope.ServiceProvider.GetRequiredService<IUserAccountDataService>();
                string currentSession = await userDataService.GetUserSessionCache(userId);
                if (currentSession != sessionId)
                    throw new RpcException(new Status(StatusCode.Unauthenticated, $"Invalid session: {sessionId}"));
            }
            await next(context);
        }
    }
}