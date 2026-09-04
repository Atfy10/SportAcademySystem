using MediatR;

namespace SportAcademy.Application.Events;

public sealed record TraineeGroupCreatedEvent(int TraineeGroupId, string TraineeGroupName, string ActorName) : INotification;
