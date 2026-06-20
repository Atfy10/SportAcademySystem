namespace SportAcademy.Domain.Entities
{
    public class RefreshToken
    {
        public int Id { get; private set; }
        public string TokenHash { get; private set; } = string.Empty;
        public string UserId { get; private set; } = string.Empty;
        public DateTime ExpiresAt { get; private set; }
        public DateTime CreatedAt { get; private set; }
        public bool IsRevoked { get; private set; }
        public DateTime? RevokedAt { get; private set; }
        public int? ReplacedByTokenId { get; private set; }

        public virtual AppUser User { get; set; } = null!;
        public virtual RefreshToken? ReplacedByToken { get; set; }

        private RefreshToken() { }

        private RefreshToken(string tokenHash, string userId, DateTime expiresAt)
        {
            TokenHash = tokenHash;
            UserId = userId;
            ExpiresAt = expiresAt;
            CreatedAt = DateTime.UtcNow;
            IsRevoked = false;
        }

        public static RefreshToken Create(string tokenHash, string userId, DateTime expiresAt)
            => new(tokenHash, userId, expiresAt);

        public void Revoke(int? replacedByTokenId = null)
        {
            IsRevoked = true;
            RevokedAt = DateTime.UtcNow;
            ReplacedByTokenId = replacedByTokenId;
        }

        public bool IsExpired()
        {
            return DateTime.UtcNow > ExpiresAt;
        }
    }
}
