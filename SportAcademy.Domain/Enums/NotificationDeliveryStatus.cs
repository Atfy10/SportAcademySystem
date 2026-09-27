namespace SportAcademy.Domain.Enums
{
    public enum NotificationDeliveryStatus
    {
        Pending = 0,
        Sent = 1,
        Failed = 2,
        Skipped = 3,
        DeadLettered = 4
    }
}
