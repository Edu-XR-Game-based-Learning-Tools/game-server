using Core.Dto;
using Microsoft.Extensions.Logging;

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

        public async Task<LoginResponse> Login(LoginRequest message)
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

                        // generate access token
                        string role = (await _userDataService.GetUserRoles(user.IdentityId)).FirstOrDefault();
                        return new LoginResponse
                        {
                            AccessToken = await _jwtFactory.GenerateEncodedToken(user.IdentityId, user.Username, role),
                            RefreshToken = refreshToken
                        };
                    }
                }
            }
            return new LoginResponse { Success = false, Message = "Invalid username or password." };
        }

        public async Task<LoginResponse> Register(RegisterRequest message)
        {
            var response = await _userDataService.Create(message);
            if (response.Success)
            {
                var user = await _userDataService.FindByName(message.Username);
                // generate refresh token
                var refreshToken = JwtUtility.GenerateToken();
                user.AddRefreshToken(refreshToken, message.RemoteIpAddress);
                await _userDataService.Update(user);

                // generate access token
                string role = (await _userDataService.GetUserRoles(user.IdentityId)).FirstOrDefault();
                return new LoginResponse
                {
                    AccessToken = await _jwtFactory.GenerateEncodedToken(user.IdentityId, user.Username, role),
                    RefreshToken = refreshToken
                };
            }
            return new LoginResponse { Success = false, Message = response.Message };
        }

        public async Task<LoginResponse> RefreshToken(ExchangeRefreshTokenRequest message)
        {
            var cp = _jwtTokenValidator.GetPrincipalFromToken(message.AccessToken);

            // invalid token/signing key was passed and we can't extract user claims
            if (cp != null)
            {
                var id = cp.Claims.First(c => c.Type == "id");
                var user = await _userDataService.Find(id.Value);

                if (user.HasValidRefreshToken(message.RefreshToken))
                {
                    string role = (await _userDataService.GetUserRoles(user.IdentityId)).FirstOrDefault();
                    var jwtToken = await _jwtFactory.GenerateEncodedToken(user.IdentityId, user.Username, role);
                    var refreshToken = JwtUtility.GenerateToken();
                    user.RemoveRefreshToken(message.RefreshToken); // delete the token we've exchanged
                    user.AddRefreshToken(refreshToken, ""); // add the new one
                    await _userDataService.Update(user);
                    return new LoginResponse
                    {
                        AccessToken = jwtToken,
                        RefreshToken = refreshToken
                    };
                }
            }
            return new LoginResponse { Success = false, Message = "Invalid token." };
        }
    }
}