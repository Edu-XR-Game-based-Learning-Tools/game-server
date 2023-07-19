using Core.EventSignal;
using Core.Framework;
using Grpc.Core;
using MagicOnion.Client;
using MessagePipe;
using Shared;
using System;
using System.Threading.Tasks;
using VContainer;

namespace Core.Network
{
    public class GRpcRetryHandlerFilter : IClientFilter
    {
        private const int MAX_RETRY_COUNT = 5;

        private readonly ErrorHandler _errorHandler;

        [Inject]
        private readonly IPublisher<OnNetworkRetryExceedMaxRetriesSignal> _onNetworkRetryExceedMaxRetriesPublisher;

        [Inject]
        protected readonly IPublisher<ShowPopupSignal> _showPopupPublisher;

        public GRpcRetryHandlerFilter(
            ErrorHandler errorHandler)
        {
            _errorHandler = errorHandler;
        }

        private async ValueTask<ResponseContext> SendAsyncInner(int retryCount, RequestContext context, Func<RequestContext, ValueTask<ResponseContext>> next)
        {
            try
            {
                return await next(context);
            }
            catch (RpcException e)
            {
                retryCount++;

                string errorMessage = e.Message;
                if (e.StatusCode == StatusCode.Unauthenticated)
                {
                    _errorHandler.ShowUnauthenticatedPopup(Defines.INVALID_SESSION, true);
                    throw new AuthenticateTokenExpiredException(_showPopupPublisher);
                }
                else if (retryCount < MAX_RETRY_COUNT)
                    return await SendAsyncInner(retryCount, context, next);

                _onNetworkRetryExceedMaxRetriesPublisher.Publish(new OnNetworkRetryExceedMaxRetriesSignal());
                throw;
            }
        }

        public async ValueTask<ResponseContext> SendAsync(RequestContext context, Func<RequestContext, ValueTask<ResponseContext>> next)
        {
            return await SendAsyncInner(0, context, next);
        }
    }
}
