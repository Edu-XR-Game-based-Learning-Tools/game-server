using MemoryPack;
using Shared.Extension;
using System;
using System.Text;

namespace Shared.Network
{
    public enum AuthType
    {
        FAILED = -2,
        UNKNOWN = -1,
        PASSWORD = 0,
        GOOGLE = 1,
        FACEBOOK = 2,
        FIREBASE = 3
    }

    [MemoryPackable]
    public partial class AuthenticationData : GeneralResponse
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public string AccountId { get; set; }
        public string UserName { get; set; }
        public byte[] AuthToken { get; set; }
        public AuthType AuthSource { get; set; }
        public DateTimeOffset Expiration { get; set; }
        public static AuthenticationData Failed { get; } = new AuthenticationData() { Success = false };
        public new string Message { get; set; } = "Failed to authenticated on the server.";

        public AuthenticationData(string userId, string username, byte[] token,
            DateTimeOffset expiration, string message = "")
        {
            Success = true;
            UserId = userId;
            UserName = username;
            AuthToken = token;
            Expiration = expiration;
            Message = message;
        }

        [MemoryPackConstructor]
        public AuthenticationData()
        {
            Message = "";
        }

        public override string ToString()
        {
            if (AuthToken == null || AuthToken.Length == 0) return "";
            string tokenStr = Encoding.ASCII.GetString(AuthToken);
            var storageToken = new string[] { UserId, tokenStr, Expiration.ToString() };
            return storageToken.Join("|");
        }

        public bool IsExpired()
        {
            return AuthToken == null || Expiration < DateTimeOffset.Now;
        }

        public AuthenticationData UpdateMessage(string msg)
        {
            Message = msg;
            return this;
        }
    }
}