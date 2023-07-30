using MessagePack;
using Newtonsoft.Json;

namespace Shared.Network
{
    [MessagePackObject(true)]
    public class RegisterRequest : LoginRequest
    {
        public string Email { get; set; }

        public string RePassword { get; set; }

        [JsonIgnore]
        public string Role { get; set; } = Constants.BasicRole;
    }
}
