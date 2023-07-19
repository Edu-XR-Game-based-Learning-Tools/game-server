using Core.Configuration;
using MagicOnion;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace Core.Service
{
    public sealed class JwtTokenValidator : IJwtTokenValidator
    {
        private readonly IJwtTokenHandler _jwtTokenHandler;
        private readonly IConfiguration _configuration;

        public JwtTokenValidator(IJwtTokenHandler jwtTokenHandler, IConfiguration configuration)
        {
            _jwtTokenHandler = jwtTokenHandler;
            _configuration = configuration;
        }

        public ClaimsPrincipal GetPrincipalFromToken(string token)
        {
            var authSettings = _configuration.GetSection(nameof(AuthSettings));
            var signingKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(authSettings[nameof(AuthSettings.Secret)]));

            return _jwtTokenHandler.ValidateToken(token, new TokenValidationParameters
            {
                ValidateLifetime = true, // we check expired tokens here
                ClockSkew = TimeSpan.Zero,

                ValidateIssuer = false,
                ValidateAudience = false,

                ValidateIssuerSigningKey = true,
                IssuerSigningKey = signingKey
            });
        }
    }
}