using MediatR;
using Moq;
using SportAcademy.Application.Commands.TenantCommands.RecordFirstDashboardLoad;
using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Contract;
using SportAcademy.Domain.Entities.Tenants;
using SportAcademy.Domain.Events;

namespace SportAcademy.Tests.Application.Handlers;

public class RecordFirstDashboardLoadCommandHandlerTests
{
    private readonly Mock<ITenantRepository> _tenantRepoMock = new();
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly Mock<IMediator> _mediatorMock = new();
    private readonly RecordFirstDashboardLoadCommandHandler _handler;

    private static readonly Guid TenantId = Guid.NewGuid();
    private static readonly Guid UserId = Guid.NewGuid();

    public RecordFirstDashboardLoadCommandHandlerTests()
    {
        _handler = new RecordFirstDashboardLoadCommandHandler(_tenantRepoMock.Object, _unitOfWorkMock.Object, _mediatorMock.Object);
    }

    private static Tenant CreateTenant(string code = "SALMYIA", DateTime? firstDashboardLoadAt = null) => new()
    {
        Id = TenantId,
        Name = "Salmiya Academy",
        DisplayName = "Salmiya Swimming Academy",
        Slug = "salmiya-academy",
        Code = code,
        FirstDashboardLoadAt = firstDashboardLoadAt,
    };

    [Fact]
    public async Task Handle_TenantNotFound_ReturnsSuccessNoOp()
    {
        _tenantRepoMock.Setup(r => r.GetByIdAsync(TenantId, It.IsAny<CancellationToken>())).ReturnsAsync((Tenant?)null);

        var result = await _handler.Handle(new RecordFirstDashboardLoadCommand(TenantId, UserId), CancellationToken.None);

        Assert.True(result.IsSuccess);
        _mediatorMock.Verify(m => m.Publish(It.IsAny<FirstTenantDashboardLoadEvent>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_AlreadyRecorded_ReturnsSuccessNoOp()
    {
        _tenantRepoMock.Setup(r => r.GetByIdAsync(TenantId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(CreateTenant(firstDashboardLoadAt: DateTime.UtcNow.AddDays(-1)));

        var result = await _handler.Handle(new RecordFirstDashboardLoadCommand(TenantId, UserId), CancellationToken.None);

        Assert.True(result.IsSuccess);
        _mediatorMock.Verify(m => m.Publish(It.IsAny<FirstTenantDashboardLoadEvent>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_SystemTenant_ReturnsSuccessNoOp()
    {
        _tenantRepoMock.Setup(r => r.GetByIdAsync(TenantId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(CreateTenant(code: Tenant.SystemTenantCode));

        var result = await _handler.Handle(new RecordFirstDashboardLoadCommand(TenantId, UserId), CancellationToken.None);

        Assert.True(result.IsSuccess);
        _mediatorMock.Verify(m => m.Publish(It.IsAny<FirstTenantDashboardLoadEvent>(), It.IsAny<CancellationToken>()), Times.Never);
        _tenantRepoMock.Verify(r => r.GetProfileAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Theory]
    [MemberData(nameof(IncompleteOnboardingCases))]
    public async Task Handle_OnboardingNotComplete_ReturnsSuccessNoOp(TenantProfile? profile)
    {
        _tenantRepoMock.Setup(r => r.GetByIdAsync(TenantId, It.IsAny<CancellationToken>())).ReturnsAsync(CreateTenant());
        _tenantRepoMock.Setup(r => r.GetProfileAsync(TenantId, It.IsAny<CancellationToken>())).ReturnsAsync(profile);

        var result = await _handler.Handle(new RecordFirstDashboardLoadCommand(TenantId, UserId), CancellationToken.None);

        Assert.True(result.IsSuccess);
        _mediatorMock.Verify(m => m.Publish(It.IsAny<FirstTenantDashboardLoadEvent>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    public static IEnumerable<object?[]> IncompleteOnboardingCases()
    {
        yield return [null];
        yield return [new TenantProfile { TenantId = TenantId, OrganizationName = "Salmiya", IsSetupComplete = false }];
    }

    [Fact]
    public async Task Handle_OnboardingComplete_SetsTimestampAndPublishesEvent()
    {
        var tenant = CreateTenant();
        _tenantRepoMock.Setup(r => r.GetByIdAsync(TenantId, It.IsAny<CancellationToken>())).ReturnsAsync(tenant);
        _tenantRepoMock.Setup(r => r.GetProfileAsync(TenantId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new TenantProfile { TenantId = TenantId, OrganizationName = "Salmiya", IsSetupComplete = true });

        var result = await _handler.Handle(new RecordFirstDashboardLoadCommand(TenantId, UserId), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.NotNull(tenant.FirstDashboardLoadAt);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        _mediatorMock.Verify(m => m.Publish(
            It.Is<FirstTenantDashboardLoadEvent>(e => e.TenantId == TenantId && e.UserId == UserId),
            It.IsAny<CancellationToken>()), Times.Once);
    }
}
