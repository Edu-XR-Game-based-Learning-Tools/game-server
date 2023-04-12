using MagicOnion;

namespace Shared.Network
{
    public interface IRpcAuthService : IService<IRpcAuthService>
    {
        UnaryResult<string> Login();

        UnaryResult<string> Register();

        UnaryResult<string> RefreshToken();
    }
}