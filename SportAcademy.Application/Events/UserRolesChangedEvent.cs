using MediatR;

namespace SportAcademy.Application.Events;

public sealed record UserRolesChangedEvent(Guid UserId, string ActorName) : INotification;
