using Core.Entity;
using Shared.Network;
using System.Security.Principal;

namespace RpcService.Authentication
{
    public readonly struct JwtAuthenticationTokenResult
    {
        public byte[] Token { get; }
        public DateTimeOffset Expiration { get; }

        public JwtAuthenticationTokenResult(byte[] token, DateTimeOffset expiration)
        {
            Token = token ?? throw new ArgumentNullException(nameof(token));
            Expiration = expiration;
        }
    }

    public class CustomJwtAuthUserIdentity : IIdentity
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public AccountType AccountType { get; set; }
        public string AccountId { get; set; }
        public string Name { get; set; }
        public string SessionId { get; set; }
        public AuthType AuthSource { get; set; }

        public bool IsAuthenticated => true;
        public string AuthenticationType => "Jwt";

        public CustomJwtAuthUserIdentity()
        { }

        public CustomJwtAuthUserIdentity(int id, string userId, string accountId, AccountType accountType, string userName, string sessionId, AuthType authType)
        {
            Id = id;
            UserId = userId;
            AccountId = accountId;
            AccountType = accountType;
            Name = userName;
            SessionId = sessionId;
            AuthSource = authType;
        }
    }
}