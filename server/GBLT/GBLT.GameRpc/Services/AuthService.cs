using Core.Service;
using MagicOnion;
using MagicOnion.Server;
using Shared.Network;

namespace RpcService.Service
{
    public class AuthService : ServiceBase<IRpcAuthService>, IRpcAuthService
    {
        private readonly IAuthService _authService;

        public AuthService(
            IAuthService authService)
        {
            _authService = authService;
        }

        public async UnaryResult<AuthenticationData> Login(LoginRequest request)
        {
            AuthenticationData response = await _authService.Login<AuthenticationData>(request);
            return response;
        }

        public async UnaryResult<AuthenticationData> Register(RegisterRequest request)
        {
            AuthenticationData response = await _authService.Register<AuthenticationData>(request);
            return response;
        }

        public async UnaryResult<AuthenticationData> RefreshToken(ExchangeRefreshTokenRequest request)
        {
            AuthenticationData response = await _authService.RefreshToken<AuthenticationData>(request);
            return response;
        }
    }
}