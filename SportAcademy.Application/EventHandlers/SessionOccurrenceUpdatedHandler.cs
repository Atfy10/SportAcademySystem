using MediatR;
using SportAcademy.Application.Events;
using SportAcademy.Application.Interfaces;

namespace SportAcademy.Application.EventHandlers;

public sealed class SessionOccurrenceUpdatedHandler : INotificationHandler<SessionOccurrenceUpdatedEvent>
{
    private readonly IRealtimeService _realtimeService;

    public SessionOccurrenceUpdatedHandler(IRealtimeService realtimeService)
    {
        _realtimeService = realtimeService;
    }

    public async Task Handle(SessionOccurrenceUpdatedEvent notification, CancellationToken cancellationToken)
    {
        await _realtimeService.SessionOccurrenceUpdated(notification.SessionOccurrenceId);
    }
}
