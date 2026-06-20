using Microsoft.AspNetCore.Identity;
using SportAcademy.Domain.Contract;

namespace SportAcademy.Domain.Entities
{
    public class AppUser : IdentityUser, IAuditableEntity, ISoftDeletable
    {
        public bool IsBanned { get; private set; }
        public DateTime CreatedAt { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string? UpdatedBy { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime? DeletedAt { get; set; }
        public string? DeletedBy { get; set; }

        public virtual Employee? Employee { get; set; }
        public virtual Trainee? Trainee { get; set; }
        public virtual Profile Profile { get; set; } = null!;
        public virtual ICollection<NotificationRecipient> Notifications { get; set; } = [];

        private AppUser() { }

        private AppUser(string userName, string email, string? phoneNumber, bool emailConfirmed)
        {
            UserName = userName;
            Email = email;
            PhoneNumber = phoneNumber;
            EmailConfirmed = emailConfirmed;
        }

        public static AppUser Create(string userName, string email, string? phoneNumber, bool emailConfirmed)
            => new(userName, email, phoneNumber, emailConfirmed);

        public static AppUser CreateWithTrainee(string userName, string email, string phoneNumber)
            => new(userName, email, phoneNumber, false);

        public static AppUser CreateForEmployee(string userName)
        {
            var user = new AppUser();
            user.UserName = userName;
            return user;
        }

        public void ToggleBanStatus()
        {
            IsBanned = !IsBanned;
        }

        public void MarkDeleted()
        {
            IsDeleted = true;
            DeletedAt = DateTime.UtcNow;
        }

        public void AssignProfile(Profile profile)
        {
            Profile = profile;
        }
    }
}
