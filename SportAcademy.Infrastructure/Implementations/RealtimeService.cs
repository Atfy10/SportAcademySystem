using Microsoft.AspNetCore.SignalR;
using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Contract;
using SportAcademy.Infrastructure.Notifications;

namespace SportAcademy.Infrastructure.Implementations;

public class RealtimeService : IRealtimeService
{
    private readonly IHubContext<NotificationHub, INotificationClient> _hubContext;
    private readonly ITenantIdProvider _tenantIdProvider;

    public RealtimeService(IHubContext<NotificationHub, INotificationClient> hubContext, ITenantIdProvider tenantIdProvider)
    {
        _hubContext = hubContext;
        _tenantIdProvider = tenantIdProvider;
    }

    public async Task AttendanceUpdated(int sessionOccurrenceId)
        => await TenantGroup().AttendanceUpdated(sessionOccurrenceId);

    public async Task SessionOccurrenceUpdated(int sessionOccurrenceId)
        => await TenantGroup().SessionOccurrenceUpdated(sessionOccurrenceId);

    public async Task EnrollmentUpdated(int enrollmentId)
        => await TenantGroup().EnrollmentUpdated(enrollmentId);

    public async Task DashboardStatsUpdated()
        => await TenantGroup().DashboardStatsUpdated();

    public async Task TraineeGroupUpdated(int traineeGroupId)
        => await TenantGroup().TraineeGroupUpdated(traineeGroupId);

    public async Task SubscriptionUpdated(int subscriptionId)
        => await TenantGroup().SubscriptionUpdated(subscriptionId);

    /// Every domain-update broadcast is scoped to the current tenant's "General" SignalR
    /// group - never Clients.All - so one tenant's real-time updates can never reach another
    /// tenant's connected clients.
    private INotificationClient TenantGroup()
    {
        var tenantId = _tenantIdProvider.TenantId
            ?? throw new InvalidOperationException("RealtimeService invoked without a resolved tenant context.");

        return _hubContext.Clients.Group(NotificationGroupNames.ForTenant(tenantId, NotificationGroupNames.General));
    }
}
