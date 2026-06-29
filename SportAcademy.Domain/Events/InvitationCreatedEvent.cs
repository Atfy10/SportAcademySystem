using MediatR;

namespace SportAcademy.Domain.Events;

public sealed record InvitationCreatedEvent(Guid InvitationId, string RawToken, string TenantSlug, string Email) : INotification;
