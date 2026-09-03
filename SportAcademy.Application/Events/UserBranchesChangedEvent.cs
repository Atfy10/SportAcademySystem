using MediatR;

namespace SportAcademy.Application.Events;

public sealed record UserBranchesChangedEvent(Guid UserId, string ActorName) : INotification;
