namespace Core.Dto
{
    public class ExchangeRefreshTokenRequest
    {
        public string AccessToken { get; }
        public string RefreshToken { get; }
        public string SigningKey { get; }
    }
}