using MediatR;
using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Entities.Tenants;
using SportAcademy.Domain.Enums;
using SportAcademy.Domain.Events;

namespace SportAcademy.Application.EventHandlers;

// Audit-only: a tenant's first login-form use can happen before onboarding is even finished
// (e.g. the Owner closes the setup wizard and logs back in later), so it isn't the milestone
// worth interrupting SuperAdmin for - see FirstTenantDashboardLoadHandler for that one. This
// handler exists purely so the login shows up in the same audit trail SuperAdmin already reviews.
public sealed class FirstTenantLoginHandler : INotificationHandler<FirstTenantLoginEvent>
{
    private readonly ITenantAuditRepository _auditRepository;
    private readonly IUserRepository _userRepository;

    public FirstTenantLoginHandler(ITenantAuditRepository auditRepository, IUserRepository userRepository)
    {
        _auditRepository = auditRepository;
        _userRepository = userRepository;
    }

    public async Task Handle(FirstTenantLoginEvent notification, CancellationToken ct)
    {
        var userDisplayName = await _userRepository.GetDisplayNameAsync(notification.UserId, ct);

        await _auditRepository.AddAsync(new TenantAuditEvent
        {
            TenantId = notification.TenantId,
            EventType = "tenant.first_login",
            Description = $"{userDisplayName} used the login page for the first time.",
            Outcome = AuditOutcome.Succeeded,
            PerformedByUserId = notification.UserId,
            PerformedBy = userDisplayName,
        }, ct);
    }
}
