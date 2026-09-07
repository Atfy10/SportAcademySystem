using MediatR;
using Microsoft.Extensions.Logging;
using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Contract;
using SportAcademy.Domain.Enums;
using SportAcademy.Domain.Events;

namespace SportAcademy.Application.EventHandlers;

public sealed class InvitationAcceptedHandler : INotificationHandler<InvitationAcceptedEvent>
{
    private readonly ILogger<InvitationAcceptedHandler> _logger;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IUserRepository _userRepository;
    private readonly INotificationService _notificationService;

    public InvitationAcceptedHandler(
        ILogger<InvitationAcceptedHandler> logger,
        IUnitOfWork unitOfWork,
        IUserRepository userRepository,
        INotificationService notificationService)
    {
        _logger = logger;
        _unitOfWork = unitOfWork;
        _userRepository = userRepository;
        _notificationService = notificationService;
    }

    public async Task Handle(InvitationAcceptedEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "InvitationAcceptedEvent: Invitation {InvitationId} accepted by User {UserId} at {AcceptedAt}.",
            notification.InvitationId, notification.UserId, DateTime.UtcNow);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // TenantCreatedHandler stamps Guid.Empty here for the system-auto-provisioned Owner
        // invitation it creates when a SuperAdmin provisions a brand-new tenant - there is no
        // human "inviter" to notify in that case (a real staff invite always carries the actual
        // inviting user's id). Sending to Guid.Empty would try to create a NotificationRecipient
        // for a user that doesn't exist, which EF can't resolve (no tracked/existing principal
        // for that FK) and throws.
        if (notification.InvitedByUserId == Guid.Empty)
            return;

        // Tell whoever sent the invitation that it was accepted - was purely a log line before,
        // invisible to anyone actually using the product.
        var accepterName = await _userRepository.GetDisplayNameAsync(notification.UserId, cancellationToken);
        await _notificationService.SendNotificationAsync(
            notification.InvitedByUserId.ToString(),
            "Invitation Accepted",
            $"{accepterName} accepted your invitation and joined.",
            NotificationType.Success);
    }
}
