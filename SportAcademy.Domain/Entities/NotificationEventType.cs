using SportAcademy.Domain.Enums;

namespace SportAcademy.Domain.Entities
{
    /// <summary>
    /// Platform-wide catalog of business event keys that can be routed through the notification
    /// channel pipeline - same tier as Feature (not ITenantScoped). Seeded from
    /// NotificationEventTypes.Catalog by AppDataSeeder.
    /// </summary>
    public class NotificationEventType
    {
        public Guid Id { get; set; }
        public required string Key { get; set; }
        public required string DisplayName { get; set; }
        public string? Description { get; set; }
        public NotificationType DefaultStyle { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
