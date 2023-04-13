namespace Core.Dto
{
    public class ExchangeRefreshTokenRequest
    {
        public string AccessToken { get; }
        public string RefreshToken { get; }
    }
}