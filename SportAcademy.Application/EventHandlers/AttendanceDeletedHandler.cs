using MediatR;
using SportAcademy.Application.Events;
using SportAcademy.Application.Interfaces;

namespace SportAcademy.Application.EventHandlers;

public sealed class AttendanceDeletedHandler : INotificationHandler<AttendanceDeletedEvent>
{
    private readonly IRealtimeService _realtimeService;

    public AttendanceDeletedHandler(IRealtimeService realtimeService)
    {
        _realtimeService = realtimeService;
    }

    public async Task Handle(AttendanceDeletedEvent notification, CancellationToken cancellationToken)
    {
        await _realtimeService.DashboardStatsUpdated();
    }
}
