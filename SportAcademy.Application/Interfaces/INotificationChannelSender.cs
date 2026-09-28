using SportAcademy.Domain.Entities;
using SportAcademy.Domain.Enums;

namespace SportAcademy.Application.Interfaces;

/// <summary>
/// One implementation per NotificationChannel, resolved by the dispatcher/worker via
/// IEnumerable&lt;INotificationChannelSender&gt; keyed by .Channel - the extension point a future
/// channel (e.g. a real Push implementation) plugs into with zero changes to the dispatcher or
/// NotificationDeliveryWorker.
/// </summary>
public interface INotificationChannelSender
{
    NotificationChannel Channel { get; }

    Task<ChannelSendResult> SendAsync(
        NotificationDelivery delivery, Notification notification, CancellationToken ct = default);
}

/// <summary>
/// Succeeded: delivery -> Sent. Not succeeded + ShouldRetry: delivery -> Failed, retried with
/// backoff up to the worker's attempt budget. Not succeeded + !ShouldRetry: delivery -> Skipped,
/// never retried (e.g. no destination to send to, or a channel that's a deliberate stub).
/// </summary>
public readonly record struct ChannelSendResult(bool Succeeded, bool ShouldRetry, string? Error);
