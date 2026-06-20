using MediatR;
using SportAcademy.Application.Events;
using SportAcademy.Application.Interfaces;

namespace SportAcademy.Application.EventHandlers;

public sealed class AttendanceCreatedHandler : INotificationHandler<AttendanceCreatedEvent>
{
    private readonly IRealtimeService _realtimeService;

    public AttendanceCreatedHandler(IRealtimeService realtimeService)
    {
        _realtimeService = realtimeService;
    }

    public async Task Handle(AttendanceCreatedEvent notification, CancellationToken cancellationToken)
    {
        await _realtimeService.AttendanceUpdated(notification.SessionOccurrenceId);
        await _realtimeService.DashboardStatsUpdated();
    }
}
