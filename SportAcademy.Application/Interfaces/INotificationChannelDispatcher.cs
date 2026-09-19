namespace SportAcademy.Application.Interfaces;

/// <summary>
/// Turns one persisted Notification + its recipient list into per-recipient channel decisions:
/// InApp fires inline (unchanged behavior - see InAppChannelSender), Email/Push/WhatsApp each get
/// a Pending NotificationDelivery row for NotificationDeliveryWorker to pick up. Eligibility per
/// recipient = the tenant's TenantNotificationChannelRule matrix (defaulted when no row exists)
/// minus that user's own UserNotificationPreference opt-outs - a user can only narrow what the
/// tenant allows, never widen it, and InApp is exempt from opt-out entirely.
/// </summary>
public interface INotificationChannelDispatcher
{
    Task DispatchAsync(
        int notificationId,
        IReadOnlyCollection<Guid> recipientUserIds,
        string eventTypeKey,
        CancellationToken ct = default);
}
