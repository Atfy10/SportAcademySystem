using MediatR;

namespace SportAcademy.Domain.Events;

public sealed record InvitationCreatedEvent(Guid InvitationId) : INotification;
