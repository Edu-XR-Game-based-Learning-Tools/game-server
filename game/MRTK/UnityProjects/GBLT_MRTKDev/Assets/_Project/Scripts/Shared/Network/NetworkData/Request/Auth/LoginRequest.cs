using MessagePack;

namespace Shared.Network
{
    [MessagePackObject(true)]
    public class LoginRequest
    {
        public string Username { get; set; }
        public string Password { get; set; }

        public string RemoteIpAddress { get; set; }
    }
}
