using MediatR;

namespace SportAcademy.Application.Events;

public sealed record TraineeGroupUpdatedEvent(int TraineeGroupId) : INotification;
