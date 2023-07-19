using Microsoft.EntityFrameworkCore;

namespace Core.Entity
{
    [Index(nameof(Username))]
    public class TUser : BaseEntity, IAggregateRoot
    {
        public string IdentityId { get; set; }
        public string Username { get; set; } // Required by automapper
        public string UserName { get; set; }
        public string Email { get; set; }
        public string PasswordHash { get; set; }

        private readonly List<TRefreshToken> _refreshTokens = new();
        public IReadOnlyCollection<TRefreshToken> RefreshTokens => _refreshTokens.AsReadOnly();

        public ICollection<TQuizCollection> QuizCollections { get; private set; }

        public bool HasValidRefreshToken(string refreshToken)
        {
            return _refreshTokens.Any(rt => rt.Token == refreshToken && rt.Active);
        }

        public void AddRefreshToken(string token, string remoteIpAddress, double daysToExpire = 5)
        {
            _refreshTokens.Add(new TRefreshToken(token, DateTime.UtcNow.AddDays(daysToExpire), remoteIpAddress));
        }

        public void RemoveRefreshToken(string refreshToken)
        {
            _refreshTokens.Remove(_refreshTokens.First(t => t.Token == refreshToken));
        }
    }
}