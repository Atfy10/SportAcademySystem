using MediatR;

namespace SportAcademy.Application.Events;

public sealed record UserActiveStatusChangedEvent(Guid UserId, bool IsBanned, string ActorName) : INotification;
