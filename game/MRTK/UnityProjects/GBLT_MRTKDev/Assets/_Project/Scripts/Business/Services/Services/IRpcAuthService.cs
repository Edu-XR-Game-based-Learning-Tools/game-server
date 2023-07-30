using Cysharp.Threading.Tasks;
using Shared.Network;

namespace Core.Business
{
    public interface IRpcAuthController
    {
        bool IsExpired { get; }

        UniTask<AuthenticationData> Login(LoginRequest request);

        UniTask<AuthenticationData> Register(RegisterRequest request);

        UniTask<AuthenticationData> RefreshToken(ExchangeRefreshTokenRequest request);
    }
}
