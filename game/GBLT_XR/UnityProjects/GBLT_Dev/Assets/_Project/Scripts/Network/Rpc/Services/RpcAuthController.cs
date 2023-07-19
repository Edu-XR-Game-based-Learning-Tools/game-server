using Core.Business;
using Cysharp.Threading.Tasks;
using MagicOnion.Client;
using Shared.Network;
using System;

namespace Core.Network
{
    public class RpcAuthController : IRpcAuthController, IDisposable
    {
        private readonly IGRpcServiceClient _gRpcServiceClient;
        private readonly UserAuthentication _userAuthentication;

        private IClientFilter[] _authenClientFilter;
        private IClientFilter[] _unAuthenClientFilter;

        public bool IsExpired => _userAuthentication == null || _userAuthentication.IsExpired;

        public RpcAuthController(
            IGRpcServiceClient gRpcServiceClient,
            UserAuthentication userAuthenticationData,
            GRpcAuthenticationFilter gRpcAuthenticationFilter,
            GRpcRetryHandlerFilter grpcErrorHandlerFilter)
        {
            _gRpcServiceClient = gRpcServiceClient;
            _userAuthentication = userAuthenticationData;
            _authenClientFilter = new IClientFilter[] { gRpcAuthenticationFilter, grpcErrorHandlerFilter };
            _unAuthenClientFilter = new[] { grpcErrorHandlerFilter };
        }

        public void Dispose()
        { }

        public void ClearAuthenticationData()
        {
            _userAuthentication.ClearAuthenticationData();
        }

        public async UniTask<AuthenticationData> Login(LoginRequest request)
        {
            AuthenticationData data = await _gRpcServiceClient.CreateServiceWithFilter<IRpcAuthService>(_unAuthenClientFilter).Login(request);
            return data;
        }

        public async UniTask<AuthenticationData> Register(RegisterRequest request)
        {
            AuthenticationData data = await _gRpcServiceClient.CreateServiceWithFilter<IRpcAuthService>(_unAuthenClientFilter).Register(request);
            return data;
        }

        public async UniTask<AuthenticationData> RefreshToken(ExchangeRefreshTokenRequest request)
        {
            AuthenticationData data = await _gRpcServiceClient.CreateServiceWithFilter<IRpcAuthService>(_unAuthenClientFilter).RefreshToken(request);
            return data;
        }
    }
}
