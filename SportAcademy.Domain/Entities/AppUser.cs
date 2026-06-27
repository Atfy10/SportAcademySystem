using Microsoft.AspNetCore.Identity;
using SportAcademy.Domain.Contract;
using SportAcademy.Domain.Entities.Tenants;

namespace SportAcademy.Domain.Entities
{
    public class AppUser : IdentityUser<Guid>, ITenantScoped, IAuditableEntity, ISoftDeletable
    {
        public bool IsPasswordReset { get; set; }
        public bool IsBanned { get; set; }
        public DateTime CreatedAt { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string? UpdatedBy { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime? DeletedAt { get; set; }
        public string? DeletedBy { get; set; }

        public Guid TenantId { get; set; }
        public Tenant Tenant { get; set; } = null!;

        // Navigation Properties
        public virtual Employee? Employee { get; set; }
        public virtual Trainee? Trainee { get; set; }
        public virtual Profile Profile { get; set; } = null!;
        public ICollection<AppUserRole> UserRoles { get; set; } = [];
        public virtual ICollection<NotificationRecipient> Notifications { get; set; } = [];
        public virtual ICollection<NotificationGroupMember> GroupMemberships { get; set; } = [];
    }
}
