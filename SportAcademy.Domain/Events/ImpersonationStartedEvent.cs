using MediatR;

namespace SportAcademy.Domain.Events;

public sealed record ImpersonationStartedEvent(
    Guid TenantId,
    Guid OwnerId,
    Guid SuperAdminUserId,
    string Reason,
    DateTime ExpiresAt) : INotification;
