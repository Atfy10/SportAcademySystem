using MediatR;

namespace SportAcademy.Domain.Events;

public sealed record InvitationAcceptedEvent(Guid InvitationId, Guid UserId) : INotification;
