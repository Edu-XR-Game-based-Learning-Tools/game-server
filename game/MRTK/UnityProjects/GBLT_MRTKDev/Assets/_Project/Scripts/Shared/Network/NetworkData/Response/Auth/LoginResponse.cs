using MessagePack;
using System;

namespace Shared.Network
{
    [Serializable]
    [MessagePackObject(true)]
    public class LoginResponse : GeneralResponse
    {
        public AccessToken AccessToken { get; set; }
        public string RefreshToken { get; set; }
        public AuthType AuthSource { get; set; } = AuthType.PASSWORD;
    }
}
