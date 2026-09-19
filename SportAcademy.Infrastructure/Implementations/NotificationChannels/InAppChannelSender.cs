using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Entities;
using SportAcademy.Domain.Enums;

namespace SportAcademy.Infrastructure.Implementations.NotificationChannels;

/// <summary>
/// InApp delivery (persist NotificationRecipient + SignalR push) already happens, unchanged, in
/// NotificationService before the dispatcher ever runs - see NotificationChannelDispatcher's own
/// remarks on why InApp is dispatched inline rather than through this sender. This exists purely
/// so INotificationChannelSender's DI collection (IEnumerable&lt;INotificationChannelSender&gt;,
/// resolved by NotificationDeliveryWorker) is complete for every NotificationChannel value; it
/// has nothing left to do by the time anything could call it.
/// </summary>
public class InAppChannelSender : INotificationChannelSender
{
    public NotificationChannel Channel => NotificationChannel.InApp;

    public Task<ChannelSendResult> SendAsync(
        NotificationDelivery delivery, Notification notification, CancellationToken ct = default)
        => Task.FromResult(new ChannelSendResult(true, false, null));
}
