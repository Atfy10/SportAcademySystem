using MediatR;

namespace SportAcademy.Domain.Events;

public sealed record TenantCreatedEvent(
    Guid TenantId,
    string TenantSlug,
    string OwnerEmail,
    string OwnerName) : INotification;
