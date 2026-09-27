using Microsoft.Extensions.Logging;
using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Entities;
using SportAcademy.Domain.Enums;

namespace SportAcademy.Infrastructure.Implementations.NotificationChannels;

/// <summary>
/// Phase 3 is abstraction-only (see IWhatsAppApiClient's remarks) - this sender is otherwise
/// real: it resolves the recipient's phone and calls through the IWhatsAppApiClient seam exactly
/// as a genuine provider integration would. With the default NotConfiguredWhatsAppApiClient
/// behind it, every send still resolves to Skipped (ShouldRetry: false) - nothing changes for the
/// outbox until a real IWhatsAppApiClient is registered.
///
/// TemplateName/parameters below are placeholders ("generic_notification" / {{title}},{{body}})
/// standing in for whatever templates actually get approved with a real provider account - no
/// approved template exists yet, so this can't be more specific than that; wiring the real
/// per-event-type template names is provider-setup work, not something this codebase can decide
/// in advance.
/// </summary>
public class WhatsAppChannelSender : INotificationChannelSender
{
    private readonly IWhatsAppApiClient _client;
    private readonly IContactResolver _contactResolver;
    private readonly ILogger<WhatsAppChannelSender> _logger;

    public WhatsAppChannelSender(
        IWhatsAppApiClient client, IContactResolver contactResolver, ILogger<WhatsAppChannelSender> logger)
    {
        _client = client;
        _contactResolver = contactResolver;
        _logger = logger;
    }

    public NotificationChannel Channel => NotificationChannel.WhatsApp;

    public async Task<ChannelSendResult> SendAsync(
        NotificationDelivery delivery, Notification notification, CancellationToken ct = default)
    {
        if (!_client.IsConfigured)
        {
            _logger.LogInformation(
                "WhatsApp provider not configured - skipping delivery {DeliveryId} to user {UserId}.",
                delivery.Id, delivery.RecipientUserId);
            return new ChannelSendResult(false, false, "No WhatsApp provider is configured yet.");
        }

        var phone = delivery.ResolvedDestination
            ?? await _contactResolver.ResolvePhoneAsync(delivery.RecipientUserId, ct);

        if (string.IsNullOrWhiteSpace(phone))
            return new ChannelSendResult(false, false, "No phone number on file for this recipient.");

        var result = await _client.SendTemplateMessageAsync(
            phone,
            templateName: "generic_notification",
            parameters: new Dictionary<string, string>
            {
                ["title"] = notification.Title ?? "Sport Academy",
                ["body"] = notification.Message,
            },
            ct);

        if (result.Succeeded) return new ChannelSendResult(true, false, null);

        // A real provider's transient failures (rate limits, timeouts) vs. permanent ones
        // (invalid number, template rejected) will need distinguishing once one exists - until
        // then every failure is treated as non-retryable, since IWhatsAppApiClient can't tell
        // the difference yet either.
        return new ChannelSendResult(false, false, result.Error);
    }
}
