using MediatR;
using SportAcademy.Application.Events;
using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Enums;

namespace SportAcademy.Application.EventHandlers;

public sealed class SubscriptionCreatedHandler : INotificationHandler<SubscriptionCreatedEvent>
{
    private readonly INotificationService _notificationService;
    private readonly IRealtimeService _realtimeService;
    private readonly IUserRepository _userRepository;
    private readonly ISubscriptionDetailsRepository _subscriptionDetailsRepository;

    public SubscriptionCreatedHandler(
        INotificationService notificationService,
        IRealtimeService realtimeService,
        IUserRepository userRepository,
        ISubscriptionDetailsRepository subscriptionDetailsRepository)
    {
        _notificationService = notificationService;
        _realtimeService = realtimeService;
        _userRepository = userRepository;
        _subscriptionDetailsRepository = subscriptionDetailsRepository;
    }

    public async Task Handle(SubscriptionCreatedEvent notification, CancellationToken cancellationToken)
    {
        // Reads the creator from the entity's own persisted CreatedBy (set by
        // AuditingInterceptor at save time) rather than the ambient IUserContextService - same
        // reasoning as the ExcuseRequest handlers: the DB fact can't drift from what was
        // actually saved.
        var subscription = await _subscriptionDetailsRepository.GetByIdAsync(notification.SubscriptionId, cancellationToken);
        var actorName = subscription?.CreatedBy is { } createdByRaw && Guid.TryParse(createdByRaw, out var createdBy)
            ? await _userRepository.GetDisplayNameAsync(createdBy, cancellationToken)
            : "System";

        await _notificationService.SendNotificationToGroupAsync(
            "Admins",
            "New Subscription",
            $"Subscription #{notification.SubscriptionId} created by {actorName}",
            NotificationType.Info);

        await _realtimeService.SubscriptionUpdated(notification.SubscriptionId);
    }
}
