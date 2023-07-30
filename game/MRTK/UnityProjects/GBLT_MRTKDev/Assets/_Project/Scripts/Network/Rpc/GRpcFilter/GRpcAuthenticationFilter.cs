using Core.EventSignal;
using Core.Framework;
using Grpc.Core;
using MagicOnion.Client;
using MessagePipe;
using Shared;
using Shared.Extension;
using System;
using System.Threading.Tasks;
using VContainer;

namespace Core.Network
{
    public class GRpcAuthenticationFilter : IClientFilter
    {
        private readonly ErrorHandler _errorHandler;
        private readonly UserAuthentication _userAuthenticationData;
        [Inject]
        protected readonly IPublisher<ShowPopupSignal> _showPopupPublisher;

        public GRpcAuthenticationFilter(
            ErrorHandler errorHandler,
            UserAuthentication userAuthenticationData)
        {
            _errorHandler = errorHandler;
            _userAuthenticationData = userAuthenticationData;
        }

        private void AddContextHeader(RequestContext context)
        {
            var entry = context.CallOptions.Headers.Find(_ => _.Key == "auth-token-bin");
            if (entry == null)
            {
                var metadataToken = new Metadata.Entry("auth-token-bin", _userAuthenticationData.Token);
                context.CallOptions.Headers.Add(metadataToken);
            }
        }

        public async ValueTask<ResponseContext> SendAsync(RequestContext context, Func<RequestContext, ValueTask<ResponseContext>> next)
        {
            if (_userAuthenticationData.IsExpired)
            {
                _errorHandler.ShowUnauthenticatedPopup(Defines.INVALID_SESSION, true);
                throw new AuthenticateTokenExpiredException(_showPopupPublisher);
            }

            AddContextHeader(context);

            return await next(context);
        }
    }
}
