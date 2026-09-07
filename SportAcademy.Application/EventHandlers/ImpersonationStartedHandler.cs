using MediatR;
using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Enums;
using SportAcademy.Domain.Events;

namespace SportAcademy.Application.EventHandlers;

// The tenant's Owner has no other way to learn a SuperAdmin looked at their account -
// StartImpersonationCommandHandler's own audit entry is only visible on the Platform side. This
// is the tenant-facing half of that same event.
public sealed class ImpersonationStartedHandler : INotificationHandler<ImpersonationStartedEvent>
{
    private readonly IUserRepository _userRepository;
    private readonly INotificationService _notificationService;

    public ImpersonationStartedHandler(IUserRepository userRepository, INotificationService notificationService)
    {
        _userRepository = userRepository;
        _notificationService = notificationService;
    }

    public async Task Handle(ImpersonationStartedEvent notification, CancellationToken cancellationToken)
    {
        var superAdminName = await _userRepository.GetDisplayNameAsync(notification.SuperAdminUserId, cancellationToken);

        await _notificationService.SendNotificationAsync(
            notification.OwnerId.ToString(),
            "Support Access Started",
            $"{superAdminName} from the platform team started viewing your account " +
            $"(reason: {notification.Reason}). This access expires at {notification.ExpiresAt:HH:mm} UTC.",
            NotificationType.Warning);
    }
}
