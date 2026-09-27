using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Contract;
using SportAcademy.Domain.Entities;
using SportAcademy.Domain.Enums;

namespace SportAcademy.Infrastructure.Implementations.NotificationChannels;

public class EmailChannelSender : INotificationChannelSender
{
    private readonly IContactResolver _contactResolver;
    private readonly IEmailService _emailService;

    public EmailChannelSender(IContactResolver contactResolver, IEmailService emailService)
    {
        _contactResolver = contactResolver;
        _emailService = emailService;
    }

    public NotificationChannel Channel => NotificationChannel.Email;

    public async Task<ChannelSendResult> SendAsync(
        NotificationDelivery delivery, Notification notification, CancellationToken ct = default)
    {
        var email = delivery.ResolvedDestination
            ?? await _contactResolver.ResolveEmailAsync(delivery.RecipientUserId, ct);

        if (string.IsNullOrWhiteSpace(email))
            return new ChannelSendResult(false, false, "No email address on file for this recipient.");

        var subject = notification.Title ?? "Notification";
        var htmlBody = $"<p>{System.Net.WebUtility.HtmlEncode(notification.Message)}</p>";

        try
        {
            await _emailService.SendAsync(email, subject, htmlBody, ct);
            return new ChannelSendResult(true, false, null);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            return new ChannelSendResult(false, true, ex.Message);
        }
    }
}
