using MediatR;

namespace SportAcademy.Domain.Events;

public sealed record FirstTenantLoginEvent(Guid TenantId, string TenantDisplayName, Guid UserId) : INotification;
