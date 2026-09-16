using FluentAssertions;
using Moq;
using SportAcademy.Application.Commands.PlatformCommands.ConfirmReconciliationBypass;
using SportAcademy.Application.Common.Limits;
using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Contract;
using SportAcademy.Domain.Entities.Tenants;
using SportAcademy.Domain.Enums;

namespace SportAcademy.Tests.Application.Handlers;

public class ConfirmReconciliationBypassCommandHandlerTests
{
    private static readonly Guid TenantId = Guid.NewGuid();
    private static readonly Guid SuperAdminId = Guid.NewGuid();

    private readonly Mock<ITenantRepository> _tenantRepoMock = new();
    private readonly Mock<IInvitationTokenService> _tokenServiceMock = new();
    private readonly Mock<IEffectiveLimitService> _limitServiceMock = new();
    private readonly Mock<IUserContextService> _userContextMock = new();
    private readonly Mock<ITenantStatusCacheInvalidator> _tenantStatusCacheMock = new();
    private readonly Mock<IRealtimeService> _realtimeServiceMock = new();
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly ConfirmReconciliationBypassCommandHandler _handler;

    public ConfirmReconciliationBypassCommandHandlerTests()
    {
        _handler = new ConfirmReconciliationBypassCommandHandler(
            _tenantRepoMock.Object, _tokenServiceMock.Object, _limitServiceMock.Object,
            _userContextMock.Object, _tenantStatusCacheMock.Object, _realtimeServiceMock.Object, _unitOfWorkMock.Object);

        _userContextMock.Setup(c => c.UserId).Returns(SuperAdminId);
        _limitServiceMock
            .Setup(s => s.GetAllAsync(TenantId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(LimitedResources.All.Select(k => new EffectiveLimit(k, null, LimitSource.Unlimited, 0, null)).ToList());
    }

    private static Tenant PendingTenant() => new()
    {
        Id = TenantId, Name = "Test", DisplayName = "Test Academy", Slug = "test",
        Status = TenantStatus.PendingLimitSelection,
    };

    private static TenantLimitReconciliation OpenReconciliationWithCode(string codeHash, int attempts = 0) => new()
    {
        Id = Guid.NewGuid(),
        TenantId = TenantId,
        RequiredResourcesJson = "{}",
        OpenedAt = DateTime.UtcNow.AddDays(-1),
        DeadlineAt = DateTime.UtcNow.AddDays(6),
        BypassCodeHash = codeHash,
        BypassCodeExpiresAt = DateTime.UtcNow.AddMinutes(10),
        BypassCodeAttempts = attempts,
    };

    [Fact]
    public async Task Handle_CorrectCode_ReactivatesTenantAndClosesReconciliationAsBypassed()
    {
        var tenant = PendingTenant();
        var reconciliation = OpenReconciliationWithCode("correct-hash");
        _tenantRepoMock.Setup(r => r.GetByIdAsync(TenantId, It.IsAny<CancellationToken>())).ReturnsAsync(tenant);
        _tenantRepoMock.Setup(r => r.GetOpenReconciliationAsync(TenantId, It.IsAny<CancellationToken>())).ReturnsAsync(reconciliation);
        _tokenServiceMock.Setup(s => s.HashToken("482910")).Returns("correct-hash");

        var result = await _handler.Handle(
            new ConfirmReconciliationBypassCommand(TenantId, "482910", "Wizard was unusable, customer escalation"), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        tenant.Status.Should().Be(TenantStatus.Active);
        reconciliation.CompletedAt.Should().NotBeNull();
        reconciliation.CompletedByUserId.Should().Be(SuperAdminId);
        reconciliation.WasBypassedBySuperAdmin.Should().BeTrue();
        reconciliation.BypassCodeHash.Should().BeNull();
        _tenantStatusCacheMock.Verify(c => c.Invalidate(TenantId), Times.Once);
        _realtimeServiceMock.Verify(r => r.NotifyTenantStatusChangedAsync(TenantId, "Active"), Times.Once);
    }

    [Fact]
    public async Task Handle_WrongCode_IncrementsAttemptsAndDoesNotReactivate()
    {
        var tenant = PendingTenant();
        var reconciliation = OpenReconciliationWithCode("correct-hash");
        _tenantRepoMock.Setup(r => r.GetByIdAsync(TenantId, It.IsAny<CancellationToken>())).ReturnsAsync(tenant);
        _tenantRepoMock.Setup(r => r.GetOpenReconciliationAsync(TenantId, It.IsAny<CancellationToken>())).ReturnsAsync(reconciliation);
        _tokenServiceMock.Setup(s => s.HashToken("000000")).Returns("wrong-hash");

        var result = await _handler.Handle(
            new ConfirmReconciliationBypassCommand(TenantId, "000000", "Some reason"), CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        tenant.Status.Should().Be(TenantStatus.PendingLimitSelection);
        reconciliation.BypassCodeAttempts.Should().Be(1);
        _realtimeServiceMock.Verify(r => r.NotifyTenantStatusChangedAsync(It.IsAny<Guid>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task Handle_TooManyAttempts_RejectsEvenTheCorrectCode()
    {
        var tenant = PendingTenant();
        var reconciliation = OpenReconciliationWithCode("correct-hash", attempts: 5);
        _tenantRepoMock.Setup(r => r.GetByIdAsync(TenantId, It.IsAny<CancellationToken>())).ReturnsAsync(tenant);
        _tenantRepoMock.Setup(r => r.GetOpenReconciliationAsync(TenantId, It.IsAny<CancellationToken>())).ReturnsAsync(reconciliation);
        _tokenServiceMock.Setup(s => s.HashToken("482910")).Returns("correct-hash");

        var result = await _handler.Handle(
            new ConfirmReconciliationBypassCommand(TenantId, "482910", "Some reason"), CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        tenant.Status.Should().Be(TenantStatus.PendingLimitSelection);
    }

    [Fact]
    public async Task Handle_ExpiredCode_ReturnsFailure()
    {
        var tenant = PendingTenant();
        var reconciliation = OpenReconciliationWithCode("correct-hash");
        reconciliation.BypassCodeExpiresAt = DateTime.UtcNow.AddMinutes(-1);
        _tenantRepoMock.Setup(r => r.GetByIdAsync(TenantId, It.IsAny<CancellationToken>())).ReturnsAsync(tenant);
        _tenantRepoMock.Setup(r => r.GetOpenReconciliationAsync(TenantId, It.IsAny<CancellationToken>())).ReturnsAsync(reconciliation);
        _tokenServiceMock.Setup(s => s.HashToken("482910")).Returns("correct-hash");

        var result = await _handler.Handle(
            new ConfirmReconciliationBypassCommand(TenantId, "482910", "Some reason"), CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        tenant.Status.Should().Be(TenantStatus.PendingLimitSelection);
    }

    [Fact]
    public async Task Handle_NoCodeWasRequested_ReturnsFailure()
    {
        var tenant = PendingTenant();
        var reconciliation = new TenantLimitReconciliation { TenantId = TenantId, RequiredResourcesJson = "{}" }; // no bypass code set
        _tenantRepoMock.Setup(r => r.GetByIdAsync(TenantId, It.IsAny<CancellationToken>())).ReturnsAsync(tenant);
        _tenantRepoMock.Setup(r => r.GetOpenReconciliationAsync(TenantId, It.IsAny<CancellationToken>())).ReturnsAsync(reconciliation);

        var result = await _handler.Handle(
            new ConfirmReconciliationBypassCommand(TenantId, "482910", "Some reason"), CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
    }
}
