using System.Net;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Enums;
using SportAcademy.Infrastructure.Options;
using WebPush;
using DomainNotification = SportAcademy.Domain.Entities.Notification;
using DomainNotificationDelivery = SportAcademy.Domain.Entities.NotificationDelivery;

namespace SportAcademy.Infrastructure.Implementations.NotificationChannels;

/// <summary>
/// Real Web Push (RFC 8030) delivery - fans out to every subscription the recipient has
/// registered (one per browser/device), since a push message targets a subscription, not a
/// user. Succeeds if at least one subscription accepted it; a subscription the push service
/// reports gone (404/410) is pruned immediately, since retrying it can never succeed.
/// </summary>
public class PushChannelSender : INotificationChannelSender
{
    private readonly IPushSubscriptionRepository _subscriptionRepository;
    private readonly WebPushSettings _settings;
    private readonly ILogger<PushChannelSender> _logger;
    private readonly WebPushClient _client = new();

    public PushChannelSender(
        IPushSubscriptionRepository subscriptionRepository,
        IOptions<WebPushSettings> settings,
        ILogger<PushChannelSender> logger)
    {
        _subscriptionRepository = subscriptionRepository;
        _settings = settings.Value;
        _logger = logger;
    }

    public NotificationChannel Channel => NotificationChannel.Push;

    public async Task<ChannelSendResult> SendAsync(
        DomainNotificationDelivery delivery, DomainNotification notification, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(_settings.PublicKey) || string.IsNullOrWhiteSpace(_settings.PrivateKey))
            return new ChannelSendResult(false, false, "Web Push VAPID keys are not configured.");

        var subscriptions = await _subscriptionRepository.GetForUserAsync(delivery.RecipientUserId, ct);
        if (subscriptions.Count == 0)
            return new ChannelSendResult(false, false, "No push subscription registered for this recipient.");

        var payload = JsonSerializer.Serialize(new
        {
            title = notification.Title ?? "Sport Academy",
            body = notification.Message,
            url = notification.ActionUrl,
        });

        var vapidDetails = new VapidDetails(_settings.Subject, _settings.PublicKey, _settings.PrivateKey);

        var succeeded = false;
        var transientFailure = false;
        string? lastError = null;

        foreach (var subscription in subscriptions)
        {
            var pushSubscription = new WebPush.PushSubscription(
                subscription.Endpoint, subscription.P256dh, subscription.Auth);

            try
            {
                await _client.SendNotificationAsync(pushSubscription, payload, vapidDetails, ct);
                succeeded = true;
            }
            catch (WebPushException ex) when (ex.StatusCode is HttpStatusCode.NotFound or HttpStatusCode.Gone)
            {
                _logger.LogInformation(
                    "Pruning expired push subscription {SubscriptionId} for user {UserId} ({StatusCode}).",
                    subscription.Id, subscription.UserId, ex.StatusCode);
                await _subscriptionRepository.RemoveAsync(subscription.Id, ct);
            }
            catch (WebPushException ex)
            {
                transientFailure = true;
                lastError = ex.Message;
            }
        }

        if (succeeded) return new ChannelSendResult(true, false, null);
        if (transientFailure) return new ChannelSendResult(false, true, lastError);

        // Every subscription was gone and got pruned above - nothing left to retry against.
        return new ChannelSendResult(false, false, "Every push subscription for this recipient was gone.");
    }
}
