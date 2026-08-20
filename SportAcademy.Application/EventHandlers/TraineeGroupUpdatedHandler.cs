using MediatR;
using SportAcademy.Application.Events;
using SportAcademy.Application.Interfaces;

namespace SportAcademy.Application.EventHandlers;

public sealed class TraineeGroupUpdatedHandler : INotificationHandler<TraineeGroupUpdatedEvent>
{
    private readonly IRealtimeService _realtimeService;

    public TraineeGroupUpdatedHandler(IRealtimeService realtimeService)
    {
        _realtimeService = realtimeService;
    }

    public async Task Handle(TraineeGroupUpdatedEvent notification, CancellationToken cancellationToken)
    {
        await _realtimeService.TraineeGroupUpdated(notification.TraineeGroupId);
    }
}
