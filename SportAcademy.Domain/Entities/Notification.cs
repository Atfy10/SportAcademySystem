using SportAcademy.Domain.Contract;
using SportAcademy.Domain.Entities.Tenants;
using SportAcademy.Domain.Enums;

namespace SportAcademy.Domain.Entities
{
    public class Notification : ITenantScoped, IAuditableEntity
    {
        public int Id { get; set; }
        public required string Message { get; set; }
        public DateTime CreatedAt { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string? UpdatedBy { get; set; }
        public string? GroupName { get; set; }
        public string? Title { get; set; }
        public NotificationType? Type { get; set; }
        public string? ActionUrl { get; set; }

        public Guid TenantId { get; set; }
        public Tenant Tenant { get; set; } = null!;

        public virtual ICollection<NotificationRecipient> Recipients { get; set; } = [];
    }
}
