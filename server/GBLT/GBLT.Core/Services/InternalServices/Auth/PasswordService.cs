using Core.Entity;
using Microsoft.Extensions.Logging;
using Shared.Network;
using System.Security.Claims;
using static Shared.Network.Enums;

namespace Core.Service
{
    public class PasswordService : IAuthService
    {
        private readonly ILogger<PasswordService> _logger;
        private readonly IUserDataService _userDataService;
        private readonly IJwtFactory _jwtFactory;
        private readonly IJwtTokenValidator _jwtTokenValidator;

        public PasswordService(
            ILogger<PasswordService> logger,
            IUserDataService userDataService,
            IJwtFactory jwtFactory,
            IJwtTokenValidator jwtTokenValidator)
        {
            _logger = logger;
            _userDataService = userDataService;
            _jwtFactory = jwtFactory;
            _jwtTokenValidator = jwtTokenValidator;
        }

        private T PrepareAuthResponse<T>(TUser user, AccessToken accessToken, string refreshToken) where T : LoginResponse, new()
        {
            T response = new()
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken
            };
            if (typeof(T) == typeof(AuthenticationData))
            {
                (response as AuthenticationData).UserId = user.EId;
                (response as AuthenticationData).UserName = user.UserName ?? user.Username;
            }

            return response;
        }

        public async Task<T> Login<T>(LoginRequest message) where T : LoginResponse, new()
        {
            if (!string.IsNullOrEmpty(message.Username) && !string.IsNullOrEmpty(message.Password))
            {
                // ensure we have a user with the given user name
                var user = await _userDataService.FindByName(message.Username);
                if (user != null)
                {
                    // validate password
                    if (await _userDataService.CheckPassword(user, message.Password))
                    {
                        // generate refresh token
                        var refreshToken = JwtUtility.GenerateToken();
                        user.AddRefreshToken(refreshToken, message.RemoteIpAddress);
                        await _userDataService.Update(user);

                        string role = (await _userDataService.GetUserRoles(user.IdentityId)).FirstOrDefault();
                        T response = PrepareAuthResponse<T>(user,
                            await _jwtFactory.GenerateEncodedToken(user.IdentityId, user.Username, role), refreshToken);
                        return response;
                    }
                }
            }
            return new T { Success = false, Message = "Invalid username or password." };
        }

        public async Task<T> Register<T>(RegisterRequest message) where T : LoginResponse, new()
        {
            var createResponse = await _userDataService.Create(message);
            if (createResponse.Success)
            {
                var user = await _userDataService.FindByName(message.Username);
                // generate refresh token
                var refreshToken = JwtUtility.GenerateToken();
                user.AddRefreshToken(refreshToken, message.RemoteIpAddress);
                await _userDataService.Update(user);

                string role = (await _userDataService.GetUserRoles(user.IdentityId)).FirstOrDefault();
                T response = PrepareAuthResponse<T>(user,
                            await _jwtFactory.GenerateEncodedToken(user.IdentityId, user.Username, role), refreshToken);
                return response;
            }
            return new T { Success = false, Message = createResponse.Message };
        }

        public async Task<T> RefreshToken<T>(ExchangeRefreshTokenRequest message) where T : LoginResponse, new()
        {
            var cp = _jwtTokenValidator.GetPrincipalFromToken(message.AccessToken);

            // invalid token/signing key was passed and we can't extract user claims
            if (cp != null)
            {
                var id = cp.Claims.First(c => c.Type == ClaimTypes.NameIdentifier);
                var user = await _userDataService.Find(id.Value);

                if (user.HasValidRefreshToken(message.RefreshToken))
                {
                    var refreshToken = JwtUtility.GenerateToken();
                    user.RemoveRefreshToken(message.RefreshToken); // delete the token we've exchanged
                    user.AddRefreshToken(refreshToken, ""); // add the new one
                    await _userDataService.Update(user);

                    string role = (await _userDataService.GetUserRoles(user.IdentityId)).FirstOrDefault();
                    T response = PrepareAuthResponse<T>(user,
                           await _jwtFactory.GenerateEncodedToken(user.IdentityId, user.Username, role), refreshToken);
                    return response;
                }
            }
            return new T { Success = false, Message = "Invalid token." };
        }
    }
}