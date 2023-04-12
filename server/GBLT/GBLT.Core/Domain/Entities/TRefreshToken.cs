namespace Core.Entity
{
    public class TRefreshToken : BaseEntity
    {
        public string Token { get; private set; }
        public DateTime Expires { get; private set; }
        public bool Active => DateTime.UtcNow <= Expires;
        public string RemoteIpAddress { get; private set; }

        public TRefreshToken(string token, DateTime expires, string remoteIpAddress)
        {
            Token = token;
            Expires = expires;
            RemoteIpAddress = remoteIpAddress;
        }
    }
}