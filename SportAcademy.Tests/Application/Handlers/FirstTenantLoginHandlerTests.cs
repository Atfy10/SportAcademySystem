using Moq;
using SportAcademy.Application.EventHandlers;
using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Entities.Tenants;
using SportAcademy.Domain.Enums;
using SportAcademy.Domain.Events;

namespace SportAcademy.Tests.Application.Handlers;

// Audit-only - see FirstTenantDashboardLoadHandlerTests for the notify-SuperAdmin milestone.
public class FirstTenantLoginHandlerTests
{
    private readonly Mock<ITenantAuditRepository> _auditRepoMock = new();
    private readonly Mock<IUserRepository> _userRepoMock = new();
    private readonly FirstTenantLoginHandler _handler;

    public FirstTenantLoginHandlerTests()
    {
        _handler = new FirstTenantLoginHandler(_auditRepoMock.Object, _userRepoMock.Object);
    }

    [Fact]
    public async Task Handle_WritesAuditEvent_ForTheLoggedInTenant()
    {
        var tenantId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        _userRepoMock.Setup(r => r.GetDisplayNameAsync(userId, It.IsAny<CancellationToken>())).ReturnsAsync("Jane Owner");

        await _handler.Handle(new FirstTenantLoginEvent(tenantId, "Salmiya Academy", userId), CancellationToken.None);

        _auditRepoMock.Verify(r => r.AddAsync(
            It.Is<TenantAuditEvent>(e =>
                e.TenantId == tenantId &&
                e.EventType == "tenant.first_login" &&
                e.Outcome == AuditOutcome.Succeeded &&
                e.PerformedByUserId == userId &&
                e.PerformedBy == "Jane Owner"),
            It.IsAny<CancellationToken>()), Times.Once);
    }
}
