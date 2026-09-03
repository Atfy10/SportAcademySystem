using MediatR;

namespace SportAcademy.Application.Events;

public sealed record ExcuseRequestCreatedEvent(int ExcuseRequestId) : INotification;
