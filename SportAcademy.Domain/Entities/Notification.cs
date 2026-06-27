using SportAcademy.Domain.Contract;
using SportAcademy.Domain.Entities.Tenants;
using SportAcademy.Domain.Enums;

namespace SportAcademy.Domain.Entities
{
    public class Notification : ITenantScoped
    {
        public int Id { get; set; }
        public required string Message { get; set; }
        public DateTime CreatedAt { get; set; }
        public string? GroupName { get; set; }
        public string? Title { get; set; }
        public NotificationType? Type { get; set; }
        public string? ActionUrl { get; set; }

        public Guid TenantId { get; set; }
        public Tenant Tenant { get; set; } = null!;

        public virtual ICollection<NotificationRecipient> Recipients { get; set; } = [];
    }
}
