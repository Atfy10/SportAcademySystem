using MediatR;

namespace SportAcademy.Application.Events;

public sealed record AttendanceDeletedEvent : INotification;
