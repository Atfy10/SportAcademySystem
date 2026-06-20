using MediatR;
using SportAcademy.Application.Events;
using SportAcademy.Application.Interfaces;

namespace SportAcademy.Application.EventHandlers;

public sealed class TraineeCreatedHandler : INotificationHandler<TraineeCreatedEvent>
{
    private readonly IRealtimeService _realtimeService;

    public TraineeCreatedHandler(IRealtimeService realtimeService)
    {
        _realtimeService = realtimeService;
    }

    public async Task Handle(TraineeCreatedEvent notification, CancellationToken cancellationToken)
    {
        await _realtimeService.DashboardStatsUpdated();
    }
}
