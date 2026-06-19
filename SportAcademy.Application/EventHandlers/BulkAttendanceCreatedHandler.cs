using MediatR;
using SportAcademy.Application.Events;
using SportAcademy.Application.Interfaces;

namespace SportAcademy.Application.EventHandlers;

public sealed class BulkAttendanceCreatedHandler : INotificationHandler<BulkAttendanceCreatedEvent>
{
    private readonly IRealtimeService _realtimeService;

    public BulkAttendanceCreatedHandler(IRealtimeService realtimeService)
    {
        _realtimeService = realtimeService;
    }

    public async Task Handle(BulkAttendanceCreatedEvent notification, CancellationToken cancellationToken)
    {
        foreach (var sessionId in notification.SessionOccurrenceIds)
            await _realtimeService.AttendanceUpdated(sessionId);

        await _realtimeService.DashboardStatsUpdated();
    }
}
