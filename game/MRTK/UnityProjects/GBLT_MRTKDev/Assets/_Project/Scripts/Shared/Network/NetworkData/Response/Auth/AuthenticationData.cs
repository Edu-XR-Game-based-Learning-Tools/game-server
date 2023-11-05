using MessagePack;
using System;

namespace Shared.Network
{
    public enum AuthType
    {
        FAILED = -2,
        UNKNOWN = -1,
        PASSWORD = 0,
        GOOGLE = 1,
        FACEBOOK = 2,
        FIREBASE = 5
    }

    [System.Serializable]
    [MessagePackObject(true)]
    public class AuthenticationData : LoginResponse
    {
        public int UserId { get; set; }
        public string UserName { get; set; }
        public DateTimeOffset Expiration => AccessToken?.IssuedAt.AddSeconds(AccessToken.ExpiresIn) ?? DateTime.MinValue;

        public AuthenticationData()
        {
        }

        public bool IsExpired()
        {
            return AccessToken == null || Expiration < DateTimeOffset.Now;
        }
    }
}
