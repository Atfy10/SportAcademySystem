using Microsoft.AspNetCore.SignalR;
using Moq;
using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Contract;
using SportAcademy.Infrastructure.Implementations;
using SportAcademy.Infrastructure.Notifications;

namespace SportAcademy.Tests.Infrastructure.Implementations;

// Regression coverage for the S1 fix: every RealtimeService broadcast must target the current
// tenant's scoped "General" SignalR group - never Clients.All - so one tenant's real-time
// updates can never reach another tenant's connected clients.
public class RealtimeServiceTests
{
    private static readonly Guid TenantId = Guid.NewGuid();
    private readonly Mock<IHubContext<NotificationHub, INotificationClient>> _hubContextMock = new();
    private readonly Mock<IHubClients<INotificationClient>> _clientsMock = new();
    private readonly Mock<INotificationClient> _clientProxyMock = new();
    private readonly Mock<ITenantIdProvider> _tenantIdProviderMock = new();
    private readonly RealtimeService _service;
    private readonly string _expectedGroup = NotificationGroupNames.ForTenant(TenantId, NotificationGroupNames.General);

    public RealtimeServiceTests()
    {
        _hubContextMock.Setup(h => h.Clients).Returns(_clientsMock.Object);
        _clientsMock.Setup(c => c.Group(It.IsAny<string>())).Returns(_clientProxyMock.Object);
        _tenantIdProviderMock.Setup(p => p.TenantId).Returns(TenantId);
        _service = new RealtimeService(_hubContextMock.Object, _tenantIdProviderMock.Object);
    }

    [Fact]
    public async Task AttendanceUpdated_BroadcastsToTenantGroup_NeverToAllClients()
    {
        await _service.AttendanceUpdated(sessionOccurrenceId: 42);

        _clientsMock.Verify(c => c.Group(_expectedGroup), Times.Once);
        _clientProxyMock.Verify(c => c.AttendanceUpdated(42), Times.Once);
        _clientsMock.VerifyGet(c => c.All, Times.Never);
    }

    [Fact]
    public async Task EnrollmentUpdated_BroadcastsToTenantGroup_NeverToAllClients()
    {
        await _service.EnrollmentUpdated(enrollmentId: 7);

        _clientsMock.Verify(c => c.Group(_expectedGroup), Times.Once);
        _clientProxyMock.Verify(c => c.EnrollmentUpdated(7), Times.Once);
        _clientsMock.VerifyGet(c => c.All, Times.Never);
    }

    [Fact]
    public async Task DashboardStatsUpdated_BroadcastsToTenantGroup_NeverToAllClients()
    {
        await _service.DashboardStatsUpdated();

        _clientsMock.Verify(c => c.Group(_expectedGroup), Times.Once);
        _clientProxyMock.Verify(c => c.DashboardStatsUpdated(), Times.Once);
        _clientsMock.VerifyGet(c => c.All, Times.Never);
    }

    [Fact]
    public async Task SubscriptionUpdated_BroadcastsToTenantGroup_NeverToAllClients()
    {
        await _service.SubscriptionUpdated(subscriptionId: 99);

        _clientsMock.Verify(c => c.Group(_expectedGroup), Times.Once);
        _clientProxyMock.Verify(c => c.SubscriptionUpdated(99), Times.Once);
        _clientsMock.VerifyGet(c => c.All, Times.Never);
    }

    [Fact]
    public async Task SessionOccurrenceUpdated_BroadcastsToTenantGroup_NeverToAllClients()
    {
        await _service.SessionOccurrenceUpdated(sessionOccurrenceId: 3);

        _clientsMock.Verify(c => c.Group(_expectedGroup), Times.Once);
        _clientProxyMock.Verify(c => c.SessionOccurrenceUpdated(3), Times.Once);
        _clientsMock.VerifyGet(c => c.All, Times.Never);
    }

    [Fact]
    public async Task TraineeGroupUpdated_BroadcastsToTenantGroup_NeverToAllClients()
    {
        await _service.TraineeGroupUpdated(traineeGroupId: 11);

        _clientsMock.Verify(c => c.Group(_expectedGroup), Times.Once);
        _clientProxyMock.Verify(c => c.TraineeGroupUpdated(11), Times.Once);
        _clientsMock.VerifyGet(c => c.All, Times.Never);
    }

    [Fact]
    public async Task DifferentTenants_ResolveToDifferentGroups()
    {
        var otherTenantId = Guid.NewGuid();
        var otherProviderMock = new Mock<ITenantIdProvider>();
        otherProviderMock.Setup(p => p.TenantId).Returns(otherTenantId);
        var otherService = new RealtimeService(_hubContextMock.Object, otherProviderMock.Object);

        await otherService.DashboardStatsUpdated();

        var otherExpectedGroup = NotificationGroupNames.ForTenant(otherTenantId, NotificationGroupNames.General);
        Assert.NotEqual(_expectedGroup, otherExpectedGroup);
        _clientsMock.Verify(c => c.Group(otherExpectedGroup), Times.Once);
    }

    [Fact]
    public async Task NoTenantContext_ThrowsInsteadOfBroadcastingUnscoped()
    {
        var noTenantProviderMock = new Mock<ITenantIdProvider>();
        noTenantProviderMock.Setup(p => p.TenantId).Returns((Guid?)null);
        var service = new RealtimeService(_hubContextMock.Object, noTenantProviderMock.Object);

        await Assert.ThrowsAsync<InvalidOperationException>(() => service.DashboardStatsUpdated());
    }
}
