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

        // The invitation itself is already committed to the database by the time this event
        // fires (CreateInvitationCommandHandler/ResendInvitationCommandHandler call
        // SaveChangesAsync before publishing it) - email delivery here is a best-effort
        // courtesy notification, not part of that transaction. SendGridEmailService throws on
        // any failure (bad/expired API key, SendGrid outage, network error, etc.), and since
        // this event is published with `await _mediator.Publish(...)`, an unhandled exception
        // here would propagate all the way back to the command and report the whole
        // invitation as failed - even though it was created successfully and its link was
        // already written to the dev-invitation-links.txt fallback file (see
        // FileLoggingEmailServiceDecorator, which logs before attempting the real send).
        // Swallow and log instead: a failed notification email should never undo or mask a
        // successful invitation.
        try
        {
            await _emailService.SendAsync(notification.Email, subject, body, cancellationToken);

            _logger.LogInformation(
                "Invitation email sent to {Email} for invitation {InvitationId}",
                notification.Email, notification.InvitationId);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogError(ex,
                "Failed to send invitation email to {Email} for invitation {InvitationId} - " +
                "the invitation itself was still created successfully.",
                notification.Email, notification.InvitationId);
        }
    }
}
