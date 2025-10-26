using Microsoft.AspNetCore.Identity;
using SportAcademy.Domain.Contract;

namespace SportAcademy.Domain.Entities
{
    public class AppUser : IdentityUser, IAuditableEntity, ISoftDeletable
    {
        public bool IsBanned { get; set; }
        public DateTime CreatedAt { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string? UpdatedBy { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime? DeletedAt { get; set; }
        public string? DeletedBy { get; set; }

        // Navigation Properties
        public virtual Employee Employee { get; set; } = null!;
        public virtual Trainee Trainee { get; set; } = null!;
        public virtual Profile Profile { get; set; } = null!;
        public virtual ICollection<NotificationRecipient> Notifications { get; set; } = [];
    }
}
