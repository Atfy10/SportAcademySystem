using MediatR;
using Microsoft.Extensions.Logging;
using SportAcademy.Domain.Contract;
using SportAcademy.Domain.Events;

namespace SportAcademy.Application.EventHandlers;

public sealed class InvitationCreatedHandler : INotificationHandler<InvitationCreatedEvent>
{
    private readonly IEmailService _emailService;
    private readonly IAppUrlProvider _appUrlProvider;
    private readonly ILogger<InvitationCreatedHandler> _logger;

    public InvitationCreatedHandler(
        IEmailService emailService,
        IAppUrlProvider appUrlProvider,
        ILogger<InvitationCreatedHandler> logger)
    {
        _emailService = emailService;
        _appUrlProvider = appUrlProvider;
        _logger = logger;
    }

    public async Task Handle(InvitationCreatedEvent notification, CancellationToken cancellationToken)
    {
        var inviteUrl = _appUrlProvider.InvitationUrl(notification.TenantSlug, notification.RawToken);

        var subject = "You've been invited to join AURA Academy";
        var body = $"""
            <h2>Welcome to AURA Academy</h2>
            <p>You have been invited to set up your organization.</p>
            <p>Click the link below to accept the invitation and create your account:</p>
            <p><a href="{inviteUrl}">{inviteUrl}</a></p>
            <p>This link will expire in 7 days.</p>
            """;

        await _emailService.SendAsync(notification.Email, subject, body, cancellationToken);

        _logger.LogInformation(
            "Invitation email sent to {Email} for invitation {InvitationId}",
            notification.Email, notification.InvitationId);
    }
}
