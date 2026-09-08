using MediatR;
using Microsoft.Extensions.Logging;
using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Contract;
using SportAcademy.Domain.Entities.Tenants;
using SportAcademy.Domain.Enums;
using SportAcademy.Domain.Events;

namespace SportAcademy.Application.EventHandlers;

// The real "tenant is up and running" milestone: audits it and notifies every SuperAdmin. Unlike
// FirstTenantLoginHandler (login-only, audit-only, can fire before onboarding is even done), this
// only fires once RecordFirstDashboardLoadCommandHandler has confirmed the onboarding wizard
// actually finished - see that handler and Tenant.FirstDashboardLoadAt's doc comment.
public sealed class FirstTenantDashboardLoadHandler : INotificationHandler<FirstTenantDashboardLoadEvent>
{
    private readonly ILogger<FirstTenantDashboardLoadHandler> _logger;
    private readonly ITenantAuditRepository _auditRepository;
    private readonly ITenantRepository _tenantRepository;
    private readonly IUserRepository _userRepository;
    private readonly INotificationService _notificationService;
    private readonly ITenantIdProvider _tenantIdProvider;

    public FirstTenantDashboardLoadHandler(
        ILogger<FirstTenantDashboardLoadHandler> logger,
        ITenantAuditRepository auditRepository,
        ITenantRepository tenantRepository,
        IUserRepository userRepository,
        INotificationService notificationService,
        ITenantIdProvider tenantIdProvider)
    {
        _logger = logger;
        _auditRepository = auditRepository;
        _tenantRepository = tenantRepository;
        _userRepository = userRepository;
        _notificationService = notificationService;
        _tenantIdProvider = tenantIdProvider;
    }

    public async Task Handle(FirstTenantDashboardLoadEvent notification, CancellationToken ct)
    {
        var userDisplayName = await _userRepository.GetDisplayNameAsync(notification.UserId, ct);

        await _auditRepository.AddAsync(new TenantAuditEvent
        {
            TenantId = notification.TenantId,
            EventType = "tenant.onboarding_completed",
            Description = $"{userDisplayName} completed onboarding and reached the dashboard for the first time.",
            Outcome = AuditOutcome.Succeeded,
            PerformedByUserId = notification.UserId,
            PerformedBy = userDisplayName,
        }, ct);

        var systemTenant = await _tenantRepository.GetBySlugAsync("system", ct);
        if (systemTenant is null)
        {
            _logger.LogWarning("FirstTenantDashboardLoadEvent: System tenant not found, cannot notify SuperAdmins.");
            return;
        }

        // See FirstTenantLoginHandler's identical comment: AppUser lookups are silently scoped
        // to whatever tenant is ambient, and SuperAdmins live in the System tenant, not the
        // customer tenant this event fired for.
        using (_tenantIdProvider.Impersonate(systemTenant.Id))
        {
            var superAdminIds = await _userRepository.GetUserIdsInRolesAsync(["SuperAdmin"], ct);
            if (superAdminIds.Count > 0)
            {
                await _notificationService.SendNotificationToUsersAsync(
                    superAdminIds,
                    "New Tenant Onboarded",
                    $"{notification.TenantDisplayName}: {userDisplayName} completed onboarding and reached the dashboard for the first time.",
                    NotificationType.Success);
            }
        }
    }
}
