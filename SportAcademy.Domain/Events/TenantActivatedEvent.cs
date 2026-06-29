using MediatR;

namespace SportAcademy.Domain.Events;

public sealed record TenantActivatedEvent(Guid TenantId) : INotification;
