﻿using Microsoft.Extensions.Options;
using Shared.Network;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Principal;

namespace Core.Service
{
    public static class JwtClaimIdentifiers
    {
        public const string Role = "role", Id = "id";
    }

    public static class JwtClaims
    {
        public const string ApiAccess = "api_access";
        public const string AdminApiAccess = "admin_api_access";
    }

    public sealed class JwtFactory : IJwtFactory
    {
        private readonly IJwtTokenHandler _jwtTokenHandler;
        private readonly JwtIssuerOptions _jwtOptions;

        public JwtFactory(IJwtTokenHandler jwtTokenHandler, IOptions<JwtIssuerOptions> jwtOptions)
        {
            _jwtTokenHandler = jwtTokenHandler;
            _jwtOptions = jwtOptions.Value;
            ThrowIfInvalidOptions(_jwtOptions);
        }

        public async Task<AccessToken> GenerateEncodedToken(string identityId, string userName, string role)
        {
            var identity = GenerateClaimsIdentity(identityId, userName, role);
            DateTime issuedAt = JwtIssuerOptions.IssuedAt;

            var claims = new[]
            {
                 new Claim(JwtRegisteredClaimNames.Sub, userName),
                 new Claim(JwtRegisteredClaimNames.Jti, await JwtIssuerOptions.JtiGenerator()),
                 new Claim(JwtRegisteredClaimNames.Iat, ToUnixEpochDate(issuedAt).ToString(), ClaimValueTypes.Integer64),
                 identity.FindFirst(JwtClaimIdentifiers.Role),
                 identity.FindFirst(JwtClaimIdentifiers.Id)
             };

            // Create the JWT security token and encode it.
            var jwt = new JwtSecurityToken(
                _jwtOptions.Issuer,
                _jwtOptions.Audience,
                claims,
                JwtIssuerOptions.NotBefore,
                _jwtOptions.Expiration,
                _jwtOptions.SigningCredentials);

            return new AccessToken() { Token = _jwtTokenHandler.WriteToken(jwt), ExpiresIn = (int)_jwtOptions.ValidFor.TotalSeconds, IssuedAt = issuedAt };
        }

        private static ClaimsIdentity GenerateClaimsIdentity(string identityId, string userName, string role)
        {
            return new ClaimsIdentity(new GenericIdentity(userName, "Token"), new[]
            {
                new Claim(JwtClaimIdentifiers.Id, identityId),
                new Claim(JwtClaimIdentifiers.Role, role)
            });
        }

        /// <returns>Date converted to seconds since Unix epoch (Jan 1, 1970, midnight UTC).</returns>
        private static long ToUnixEpochDate(DateTime date)
          => (long)Math.Round((date.ToUniversalTime() -
                               new DateTimeOffset(1970, 1, 1, 0, 0, 0, TimeSpan.Zero))
                              .TotalSeconds);

        private static void ThrowIfInvalidOptions(JwtIssuerOptions options)
        {
            if (options == null) throw new ArgumentNullException(nameof(options));

            if (options.ValidFor <= TimeSpan.Zero)
            {
                throw new ArgumentException("Must be a non-zero TimeSpan.", nameof(JwtIssuerOptions.ValidFor));
            }

            if (options.SigningCredentials == null)
            {
                throw new ArgumentNullException(nameof(JwtIssuerOptions.SigningCredentials));
            }

            if (JwtIssuerOptions.JtiGenerator == null)
            {
                throw new ArgumentNullException(nameof(JwtIssuerOptions.JtiGenerator));
            }
        }
    }
}