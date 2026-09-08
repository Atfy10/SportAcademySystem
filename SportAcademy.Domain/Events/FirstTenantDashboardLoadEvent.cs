using MediatR;

namespace SportAcademy.Domain.Events;

public sealed record FirstTenantDashboardLoadEvent(Guid TenantId, string TenantDisplayName, Guid UserId) : INotification;
