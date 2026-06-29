using MediatR;
using Microsoft.Extensions.Logging;
using SportAcademy.Domain.Events;

namespace SportAcademy.Application.EventHandlers;

public sealed class InvitationAcceptedHandler : INotificationHandler<InvitationAcceptedEvent>
{
    private readonly ILogger<InvitationAcceptedHandler> _logger;

    public InvitationAcceptedHandler(ILogger<InvitationAcceptedHandler> logger)
    {
        _logger = logger;
    }

    public Task Handle(InvitationAcceptedEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "InvitationAcceptedEvent: Invitation {InvitationId} accepted by User {UserId}.",
            notification.InvitationId, notification.UserId);

        return Task.CompletedTask;
    }
}
