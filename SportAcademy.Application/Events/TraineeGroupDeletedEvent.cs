using MediatR;

namespace SportAcademy.Application.Events;

public sealed record TraineeGroupDeletedEvent(int TraineeGroupId, string TraineeGroupName, string ActorName) : INotification;
