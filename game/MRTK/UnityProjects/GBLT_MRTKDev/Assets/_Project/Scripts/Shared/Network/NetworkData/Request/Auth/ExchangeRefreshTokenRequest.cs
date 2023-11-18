using MessagePack;

namespace Shared.Network
{
    [System.Serializable]
    [MessagePackObject(true)]
    public class ExchangeRefreshTokenRequest
    {
        public string AccessToken { get; }
        public string RefreshToken { get; }
    }
}
