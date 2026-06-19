using MediatR;

namespace SportAcademy.Application.Events;

public sealed record AttendanceCreatedEvent(int SessionOccurrenceId) : INotification;
