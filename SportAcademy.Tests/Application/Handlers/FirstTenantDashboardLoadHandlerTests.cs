using Moq;
using Microsoft.Extensions.Logging;
using SportAcademy.Application.EventHandlers;
using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Contract;
using SportAcademy.Domain.Entities.Tenants;
using SportAcademy.Domain.Enums;
using SportAcademy.Domain.Events;

namespace SportAcademy.Tests.Application.Handlers;

public class FirstTenantDashboardLoadHandlerTests
{
    private readonly Mock<ITenantAuditRepository> _auditRepoMock = new();
    private readonly Mock<ITenantRepository> _tenantRepoMock = new();
    private readonly Mock<IUserRepository> _userRepoMock = new();
    private readonly Mock<INotificationService> _notificationServiceMock = new();
    private readonly Mock<ITenantIdProvider> _tenantIdProviderMock = new();
    private readonly FirstTenantDashboardLoadHandler _handler;

    private static readonly Guid SystemTenantId = Guid.NewGuid();

    public FirstTenantDashboardLoadHandlerTests()
    {
        _tenantRepoMock
            .Setup(r => r.GetBySlugAsync("system", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Tenant { Id = SystemTenantId, Name = "System", DisplayName = "System", Slug = "system", Code = Tenant.SystemTenantCode });
        _tenantIdProviderMock
            .Setup(p => p.Impersonate(SystemTenantId))
            .Returns(Mock.Of<IDisposable>());
        _userRepoMock
            .Setup(r => r.GetUserIdsInRolesAsync(It.IsAny<IEnumerable<string>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        _handler = new FirstTenantDashboardLoadHandler(
            Mock.Of<ILogger<FirstTenantDashboardLoadHandler>>(),
            _auditRepoMock.Object,
            _tenantRepoMock.Object,
            _userRepoMock.Object,
            _notificationServiceMock.Object,
            _tenantIdProviderMock.Object);
    }

    [Fact]
    public async Task Handle_WritesAuditEvent_ForTheOnboardedTenant()
    {
        var tenantId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        _userRepoMock.Setup(r => r.GetDisplayNameAsync(userId, It.IsAny<CancellationToken>())).ReturnsAsync("Jane Owner");

        await _handler.Handle(new FirstTenantDashboardLoadEvent(tenantId, "Salmiya Academy", userId), CancellationToken.None);

        _auditRepoMock.Verify(r => r.AddAsync(
            It.Is<TenantAuditEvent>(e =>
                e.TenantId == tenantId &&
                e.EventType == "tenant.onboarding_completed" &&
                e.Outcome == AuditOutcome.Succeeded &&
                e.PerformedByUserId == userId &&
                e.PerformedBy == "Jane Owner"),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_NotifiesSuperAdmins_ResolvedUnderSystemTenantContext()
    {
        var tenantId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var superAdminIds = new List<Guid> { Guid.NewGuid(), Guid.NewGuid() };
        _userRepoMock.Setup(r => r.GetDisplayNameAsync(userId, It.IsAny<CancellationToken>())).ReturnsAsync("Jane Owner");
        _userRepoMock
            .Setup(r => r.GetUserIdsInRolesAsync(
                It.Is<IEnumerable<string>>(roles => roles.SequenceEqual(new[] { "SuperAdmin" })),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(superAdminIds);

        await _handler.Handle(new FirstTenantDashboardLoadEvent(tenantId, "Salmiya Academy", userId), CancellationToken.None);

        _tenantIdProviderMock.Verify(p => p.Impersonate(SystemTenantId), Times.Once);
        _notificationServiceMock.Verify(n => n.SendNotificationToUsersAsync(
            superAdminIds,
            It.IsAny<string>(),
            It.Is<string>(m => m.Contains("Salmiya Academy") && m.Contains("Jane Owner")),
            NotificationType.Success), Times.Once);
    }

    [Fact]
    public async Task Handle_NoSuperAdmins_DoesNotCallNotificationService()
    {
        var userId = Guid.NewGuid();
        _userRepoMock.Setup(r => r.GetDisplayNameAsync(userId, It.IsAny<CancellationToken>())).ReturnsAsync("Jane Owner");

        await _handler.Handle(new FirstTenantDashboardLoadEvent(Guid.NewGuid(), "Salmiya Academy", userId), CancellationToken.None);

        _notificationServiceMock.Verify(n => n.SendNotificationToUsersAsync(
            It.IsAny<IEnumerable<Guid>>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<NotificationType>()), Times.Never);
    }
}
