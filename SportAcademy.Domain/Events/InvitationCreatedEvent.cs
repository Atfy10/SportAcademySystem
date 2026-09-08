using MediatR;

namespace SportAcademy.Domain.Events;

// Deliberately slim compared to before: it no longer carries a RawToken/TenantSlug, because
// nothing auto-emails an invitation on creation any more (see CreateInvitationCommandHandler) -
// InvitationCreatedNotificationHandler is its only subscriber, and only needs enough to write
// an in-app "an invitation was sent" notice for the tenant's existing Admins/Owners.
public sealed record InvitationCreatedEvent(Guid InvitationId, string Email, string ActorName) : INotification;
