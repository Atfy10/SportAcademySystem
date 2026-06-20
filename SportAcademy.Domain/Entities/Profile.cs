namespace SportAcademy.Domain.Entities
{
    public class Profile
    {
        public string AppUserId { get; private set; } = null!;
        public string? ProfileImageUrl { get; private set; }
        public string? Bio { get; private set; }
        public DateTime CreatedAt { get; private set; }

        public virtual AppUser User { get; set; } = null!;

        private Profile() { }

        private Profile(string appUserId, string? profileImageUrl, string? bio)
        {
            AppUserId = appUserId;
            ProfileImageUrl = profileImageUrl;
            Bio = bio;
            CreatedAt = DateTime.UtcNow;
        }

        public static Profile Create(string appUserId, string? profileImageUrl = null, string? bio = null)
            => new(appUserId, profileImageUrl, bio);
    }
}
