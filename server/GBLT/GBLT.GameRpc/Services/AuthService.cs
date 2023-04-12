using Core.Service;
using MagicOnion;
using MagicOnion.Server;
using Microsoft.AspNetCore.Authorization;
using Shared.Network;

namespace RpcService.Service
{
    [Authorize]
    public class AuthService : ServiceBase<IRpcAuthService>, IRpcAuthService
    {
        private readonly IAuthService _authService;

        public AuthService(
            IAuthService authService)
        {
            _authService = authService;
        }

        public async UnaryResult<string> Login()
        {
            return "";
        }

        [AllowAnonymous]
        public async UnaryResult<string> Register()
        {
            return "";
        }

        [AllowAnonymous]
        public async UnaryResult<string> RefreshToken()
        {
            return "";
        }
    }
}