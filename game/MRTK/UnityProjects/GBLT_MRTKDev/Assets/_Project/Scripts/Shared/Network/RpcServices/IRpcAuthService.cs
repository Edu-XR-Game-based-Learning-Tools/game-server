using MagicOnion;

namespace Shared.Network
{
    public interface IRpcAuthService : IService<IRpcAuthService>
    {
        UnaryResult<AuthenticationData> Login(LoginRequest request);

        UnaryResult<AuthenticationData> Register(RegisterRequest request);

        UnaryResult<AuthenticationData> RefreshToken(ExchangeRefreshTokenRequest request);
    }
}
