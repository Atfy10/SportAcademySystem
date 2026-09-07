using FluentAssertions;
using Moq;
using SportAcademy.Application.Commands.PlatformCommands.StartImpersonation;
using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Contract;
using SportAcademy.Domain.Entities;
using SportAcademy.Domain.Entities.Tenants;
using SportAcademy.Domain.Enums;

namespace SportAcademy.Tests.Application.Handlers;

public class StartImpersonationCommandHandlerTests
{
    private readonly Mock<ITenantRepository> _tenantRepoMock = new();
    private readonly Mock<IImpersonationGrantRepository> _grantRepoMock = new();
    private readonly Mock<IUserRepository> _userRepoMock = new();
    private readonly Mock<IUserContextService> _userContextMock = new();
    private readonly Mock<IJwtTokenService> _jwtTokenServiceMock = new();
    private readonly StartImpersonationCommandHandler _handler;

    private readonly Guid _superAdminId = Guid.NewGuid();

    public StartImpersonationCommandHandlerTests()
    {
        _userContextMock.Setup(c => c.UserId).Returns(_superAdminId);
        _handler = new StartImpersonationCommandHandler(
            _tenantRepoMock.Object,
            _grantRepoMock.Object,
            _userRepoMock.Object,
            _userContextMock.Object,
            _jwtTokenServiceMock.Object);
    }

    private static Tenant CreateTenant(Guid id, TenantStatus status) => new()
    {
        Id = id,
        Name = "Test Academy",
        DisplayName = "Test Academy",
        Slug = "test-academy",
        Status = status,
    };

    [Fact]
    public async Task Handle_TenantNotFound_ReturnsFailure404()
    {
        var tenantId = Guid.NewGuid();
        _tenantRepoMock.Setup(r => r.GetByIdAsync(tenantId, It.IsAny<CancellationToken>())).ReturnsAsync((Tenant?)null);

        var result = await _handler.Handle(new StartImpersonationCommand(tenantId, "Support ticket #123"), CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(404);
    }

    [Theory]
    [InlineData(TenantStatus.Suspended)]
    [InlineData(TenantStatus.Archived)]
    [InlineData(TenantStatus.Inactive)]
    [InlineData(TenantStatus.PendingSetup)]
    public async Task Handle_NonActiveTenant_ReturnsFailure(TenantStatus status)
    {
        var tenantId = Guid.NewGuid();
        _tenantRepoMock.Setup(r => r.GetByIdAsync(tenantId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(CreateTenant(tenantId, status));

        var result = await _handler.Handle(new StartImpersonationCommand(tenantId, "reason"), CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(400);
        _grantRepoMock.Verify(r => r.AddAsync(It.IsAny<TenantImpersonationGrant>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ActiveTenant_CreatesGrantAndIssuesToken()
    {
        var tenantId = Guid.NewGuid();
        var tenant = CreateTenant(tenantId, TenantStatus.Active);
        var superAdmin = new AppUser { Id = _superAdminId, UserName = "superadmin", Email = "sa@test.com" };

        _tenantRepoMock.Setup(r => r.GetByIdAsync(tenantId, It.IsAny<CancellationToken>())).ReturnsAsync(tenant);
        _userRepoMock.Setup(r => r.GetByIdAsync(_superAdminId, It.IsAny<CancellationToken>())).ReturnsAsync(superAdmin);

        TenantImpersonationGrant? capturedGrant = null;
        _grantRepoMock
            .Setup(r => r.AddAsync(It.IsAny<TenantImpersonationGrant>(), It.IsAny<CancellationToken>()))
            .Callback<TenantImpersonationGrant, CancellationToken>((g, _) => capturedGrant = g)
            .Returns(Task.CompletedTask);

        _jwtTokenServiceMock
            .Setup(j => j.GenerateImpersonationToken(superAdmin, tenantId, It.IsAny<Guid>(), It.IsAny<DateTime>()))
            .ReturnsAsync("impersonation-jwt");

        var result = await _handler.Handle(new StartImpersonationCommand(tenantId, "Support ticket #123"), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Data!.AccessToken.Should().Be("impersonation-jwt");
        result.Data.TenantId.Should().Be(tenantId);
        result.Data.TenantDisplayName.Should().Be("Test Academy");

        capturedGrant.Should().NotBeNull();
        capturedGrant!.TenantId.Should().Be(tenantId);
        capturedGrant.GrantedByUserId.Should().Be(_superAdminId);
        capturedGrant.Reason.Should().Be("Support ticket #123");
        capturedGrant.EndedAt.Should().BeNull();

        // Capped at 60 minutes regardless of anything the caller might ask for later.
        (capturedGrant.ExpiresAt - capturedGrant.StartedAt).Should().BeCloseTo(TimeSpan.FromMinutes(60), TimeSpan.FromSeconds(5));
    }
}
