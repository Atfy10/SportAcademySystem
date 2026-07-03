using MediatR;
using Microsoft.Extensions.Logging;
using SportAcademy.Domain.Contract;
using SportAcademy.Domain.Events;

namespace SportAcademy.Application.EventHandlers;

public sealed class InvitationAcceptedHandler : INotificationHandler<InvitationAcceptedEvent>
{
    private readonly ILogger<InvitationAcceptedHandler> _logger;
    private readonly IUnitOfWork _unitOfWork;

    public InvitationAcceptedHandler(
        ILogger<InvitationAcceptedHandler> logger,
        IUnitOfWork unitOfWork)
    {
        _logger = logger;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(InvitationAcceptedEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "InvitationAcceptedEvent: Invitation {InvitationId} accepted by User {UserId} at {AcceptedAt}.",
            notification.InvitationId, notification.UserId, DateTime.UtcNow);

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
