using MediatR;

namespace SportAcademy.Application.Events;

public sealed record BulkAttendanceCreatedEvent(IReadOnlySet<int> SessionOccurrenceIds) : INotification;
