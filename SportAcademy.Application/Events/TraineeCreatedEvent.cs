using MediatR;

namespace SportAcademy.Application.Events;

public sealed record TraineeCreatedEvent(int TraineeId) : INotification;
