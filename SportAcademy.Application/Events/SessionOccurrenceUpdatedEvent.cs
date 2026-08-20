using MediatR;

namespace SportAcademy.Application.Events;

public sealed record SessionOccurrenceUpdatedEvent(int SessionOccurrenceId) : INotification;
