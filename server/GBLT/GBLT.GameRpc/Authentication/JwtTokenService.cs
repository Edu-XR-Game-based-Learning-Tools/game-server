using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace RpcService.Authentication
{
    public class JwtTokenService
    {
        private readonly JwtTokenServiceOptions _jwtAuthOptions;
        private readonly SymmetricSecurityKey _securityKey;

        public JwtTokenService(IOptions<JwtTokenServiceOptions> jwtTokenServiceOptions)
        {
            _securityKey = new SymmetricSecurityKey(Convert.FromBase64String(jwtTokenServiceOptions.Value.Secret));
        }

        public JwtAuthenticationTokenResult CreateToken(CustomJwtAuthUserIdentity jwt)
        {
            var jwtTokenHandler = new JwtSecurityTokenHandler();
            var expires = DateTime.UtcNow.AddDays(_jwtAuthOptions.AuthTokenExpireHour);
            var token = jwtTokenHandler.CreateEncodedJwt(new SecurityTokenDescriptor()
            {
                SigningCredentials = new SigningCredentials(_securityKey, SecurityAlgorithms.HmacSha256),
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.Name, jwt.Name),
                    new Claim(ClaimTypes.NameIdentifier, jwt.UserId.ToString()),
                }),
                Expires = expires,
            });

            return new JwtAuthenticationTokenResult(
                token: Encoding.ASCII.GetBytes(token),
                expiration: expires);
        }
    }

    public class JwtTokenServiceOptions
    {
        public string Secret { get; set; }
        public int AuthTokenExpireHour { get; set; }
    }
}