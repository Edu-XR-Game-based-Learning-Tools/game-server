using MessagePack;

namespace Shared.Network
{
    [MessagePackObject(true)]
    public class ExchangeRefreshTokenRequest
    {
        public string AccessToken { get; }
        public string RefreshToken { get; }
    }
}
