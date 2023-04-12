namespace Core.Dto
{
    [Serializable]
    public sealed class AccessToken
    {
        public string Token { get; set; }
        public int ExpiresIn { get; set; }
        public DateTime IssuedAt { get; set; }

        public AccessToken(string token, int expiresIn, DateTime issuedAt)
        {
            Token = token;
            ExpiresIn = expiresIn;
            IssuedAt = issuedAt;
        }
    }
}