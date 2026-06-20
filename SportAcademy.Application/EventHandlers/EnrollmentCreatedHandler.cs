using MediatR;
using SportAcademy.Application.Events;
using SportAcademy.Application.Interfaces;

namespace SportAcademy.Application.EventHandlers;

public sealed class EnrollmentCreatedHandler : INotificationHandler<EnrollmentCreatedEvent>
{
    private readonly IRealtimeService _realtimeService;

    public EnrollmentCreatedHandler(IRealtimeService realtimeService)
    {
        _realtimeService = realtimeService;
    }

    public async Task Handle(EnrollmentCreatedEvent notification, CancellationToken cancellationToken)
    {
        await _realtimeService.EnrollmentUpdated(notification.EnrollmentId);
        await _realtimeService.DashboardStatsUpdated();
    }
}
