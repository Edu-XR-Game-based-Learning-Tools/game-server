using Core.Dto;

namespace Core.Service
{
    public interface IAuthService
    {
        Task<LoginResponse> Login(LoginRequest message);

        Task<LoginResponse> Register(RegisterRequest message);

        Task<LoginResponse> RefreshToken(ExchangeRefreshTokenRequest message);
    }
}