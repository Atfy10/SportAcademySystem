using FluentAssertions;
using Moq;
using SportAcademy.Application.Commands.PlatformCommands.RequestReconciliationBypass;
using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Contract;
using SportAcademy.Domain.Entities;
using SportAcademy.Domain.Entities.Tenants;
using SportAcademy.Domain.Enums;
using SportAcademy.Domain.Exceptions.BaseExceptions;

namespace SportAcademy.Tests.Application.Handlers;

public class RequestReconciliationBypassCommandHandlerTests
{
    private static readonly Guid TenantId = Guid.NewGuid();
    private static readonly Guid SuperAdminId = Guid.NewGuid();

    private readonly Mock<ITenantRepository> _tenantRepoMock = new();
    private readonly Mock<IUserRepository> _userRepoMock = new();
    private readonly Mock<IUserContextService> _userContextMock = new();
    private readonly Mock<IInvitationTokenService> _tokenServiceMock = new();
    private readonly Mock<IEmailService> _emailServiceMock = new();
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly RequestReconciliationBypassCommandHandler _handler;

    public RequestReconciliationBypassCommandHandlerTests()
    {
        _handler = new RequestReconciliationBypassCommandHandler(
            _tenantRepoMock.Object, _userRepoMock.Object, _userContextMock.Object,
            _tokenServiceMock.Object, _emailServiceMock.Object, _unitOfWorkMock.Object);

        _userContextMock.Setup(c => c.UserId).Returns(SuperAdminId);
    }

    private static Tenant PendingTenant() => new()
    {
        Id = TenantId, Name = "Test", DisplayName = "Test Academy", Slug = "test",
        Status = TenantStatus.PendingLimitSelection,
    };

    [Fact]
    public async Task Handle_TenantNotPendingLimitSelection_ReturnsFailure()
    {
        var tenant = new Tenant { Id = TenantId, Name = "Test", DisplayName = "Test", Slug = "test", Status = TenantStatus.Active };
        _tenantRepoMock.Setup(r => r.GetByIdAsync(TenantId, It.IsAny<CancellationToken>())).ReturnsAsync(tenant);

        var result = await _handler.Handle(new RequestReconciliationBypassCommand(TenantId), CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        _emailServiceMock.Verify(e => e.SendAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_SuperAdminHasNoEmail_ReturnsFailure()
    {
        var tenant = PendingTenant();
        _tenantRepoMock.Setup(r => r.GetByIdAsync(TenantId, It.IsAny<CancellationToken>())).ReturnsAsync(tenant);
        _tenantRepoMock.Setup(r => r.GetOpenReconciliationAsync(TenantId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new TenantLimitReconciliation { TenantId = TenantId, RequiredResourcesJson = "{}" });
        _userRepoMock.Setup(r => r.GetByIdAsync(SuperAdminId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new AppUser { Id = SuperAdminId, UserName = "sa", Email = "" });

        var result = await _handler.Handle(new RequestReconciliationBypassCommand(TenantId), CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        _emailServiceMock.Verify(e => e.SendAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ValidRequest_EmailsCodeToTheActingSuperAdminsOwnAddress()
    {
        // Not the tenant's own email - the whole point is confirming the SuperAdmin's own
        // deliberate intent, not notifying the tenant.
        var tenant = PendingTenant();
        var reconciliation = new TenantLimitReconciliation { TenantId = TenantId, RequiredResourcesJson = "{}" };
        _tenantRepoMock.Setup(r => r.GetByIdAsync(TenantId, It.IsAny<CancellationToken>())).ReturnsAsync(tenant);
        _tenantRepoMock.Setup(r => r.GetOpenReconciliationAsync(TenantId, It.IsAny<CancellationToken>())).ReturnsAsync(reconciliation);
        _userRepoMock.Setup(r => r.GetByIdAsync(SuperAdminId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new AppUser { Id = SuperAdminId, UserName = "sa", Email = "superadmin@aura.example" });
        _tokenServiceMock.Setup(s => s.GenerateNumericCode(6)).Returns("482910");
        _tokenServiceMock.Setup(s => s.HashToken("482910")).Returns("hashed-code");

        var result = await _handler.Handle(new RequestReconciliationBypassCommand(TenantId), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        reconciliation.BypassCodeHash.Should().Be("hashed-code");
        reconciliation.BypassCodeExpiresAt.Should().NotBeNull();
        reconciliation.BypassCodeAttempts.Should().Be(0);
        _emailServiceMock.Verify(
            e => e.SendAsync("superadmin@aura.example", It.IsAny<string>(), It.Is<string>(b => b.Contains("482910")), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_NoOpenReconciliation_ReturnsFailure()
    {
        var tenant = PendingTenant();
        _tenantRepoMock.Setup(r => r.GetByIdAsync(TenantId, It.IsAny<CancellationToken>())).ReturnsAsync(tenant);
        _tenantRepoMock.Setup(r => r.GetOpenReconciliationAsync(TenantId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((TenantLimitReconciliation?)null);

        var result = await _handler.Handle(new RequestReconciliationBypassCommand(TenantId), CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_NoAuthenticatedCaller_Throws()
    {
        _userContextMock.Setup(c => c.UserId).Returns((Guid?)null);
        var tenant = PendingTenant();
        _tenantRepoMock.Setup(r => r.GetByIdAsync(TenantId, It.IsAny<CancellationToken>())).ReturnsAsync(tenant);
        _tenantRepoMock.Setup(r => r.GetOpenReconciliationAsync(TenantId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new TenantLimitReconciliation { TenantId = TenantId, RequiredResourcesJson = "{}" });

        var act = () => _handler.Handle(new RequestReconciliationBypassCommand(TenantId), CancellationToken.None);
        await act.Should().ThrowAsync<InvalidOperationException>();
    }
}
