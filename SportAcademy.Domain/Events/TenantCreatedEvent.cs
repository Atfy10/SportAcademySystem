using MediatR;

namespace SportAcademy.Domain.Events;

public sealed record TenantCreatedEvent(Guid TenantId) : INotification;
