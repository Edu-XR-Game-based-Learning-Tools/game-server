using Shared.Network;

namespace Core.Service
{
    public interface IAuthService
    {
        Task<T> Login<T>(LoginRequest message) where T : LoginResponse, new();

        Task<T> Register<T>(RegisterRequest message) where T : LoginResponse, new();

        Task<T> RefreshToken<T>(ExchangeRefreshTokenRequest message)where T : LoginResponse, new();
    }
}