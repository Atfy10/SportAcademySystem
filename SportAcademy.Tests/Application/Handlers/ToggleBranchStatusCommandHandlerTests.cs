using FluentAssertions;
using Moq;
using SportAcademy.Application.Commands.BranchCommands.ToggleBranchStatus;
using SportAcademy.Application.Common.Limits;
using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Entities;
using SportAcademy.Domain.Enums;
using SportAcademy.Domain.Exceptions.BranchExceptions;

namespace SportAcademy.Tests.Application.Handlers;

public class ToggleBranchStatusCommandHandlerTests
{
    private static readonly Guid TenantId = Guid.NewGuid();

    private readonly Mock<IBranchRepository> _branchRepoMock = new();
    private readonly Mock<IUserContextService> _userContextMock = new();
    private readonly Mock<IEffectiveLimitService> _limitServiceMock = new();
    private readonly ToggleBranchStatusCommandHandler _handler;

    public ToggleBranchStatusCommandHandlerTests()
    {
        _userContextMock.Setup(c => c.TenantId).Returns(TenantId);

        // Unlimited by default - tests that specifically want LIMIT_EXCEEDED override this.
        _limitServiceMock
            .Setup(s => s.GetAsync(TenantId, LimitedResources.Branches, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new EffectiveLimit(LimitedResources.Branches, null, LimitSource.Unlimited, 0, null));

        _handler = new ToggleBranchStatusCommandHandler(
            _branchRepoMock.Object, _userContextMock.Object, _limitServiceMock.Object);
    }

    private static ToggleBranchStatusCommand CreateValidCommand(int id = 1) => new(Id: id);

    private static Branch CreateBranch(int id, bool isActive) => new()
    {
        Id = id,
        Name = "Test Branch",
        City = "Test City",
        Country = "Test Country",
        PhoneNumber = "0000000000",
        IsActive = isActive,
    };

    [Fact]
    public async Task Handle_DeactivatingActiveBranch_Succeeds()
    {
        var command = CreateValidCommand(1);
        _branchRepoMock.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(CreateBranch(1, isActive: true));
        _branchRepoMock.Setup(r => r.ToggleIsActiveAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(false);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Data.Should().BeFalse();
        _limitServiceMock.Verify(
            s => s.GetAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never,
            "deactivating never consumes a slot, so it must never be checked against the cap");
    }

    [Fact]
    public async Task Handle_ReactivatingBranch_WithHeadroom_Succeeds()
    {
        var command = CreateValidCommand(1);
        _branchRepoMock.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(CreateBranch(1, isActive: false));
        _branchRepoMock.Setup(r => r.ToggleIsActiveAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(true);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Data.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_ReactivatingBranch_AtCap_ReturnsLimitExceeded()
    {
        // A tenant deactivating branches to comply with a downgrade, then trying to just flip
        // one back on, must be blocked exactly like creating a new one would be - otherwise the
        // whole downgrade-reconciliation wizard is pointless (PLAN_LIMITS_DESIGN.md R4).
        var command = CreateValidCommand(1);
        _branchRepoMock.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(CreateBranch(1, isActive: false));
        _limitServiceMock
            .Setup(s => s.GetAsync(TenantId, LimitedResources.Branches, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new EffectiveLimit(LimitedResources.Branches, 1, LimitSource.Plan, 1, null));

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(403);
        result.Errors!["code"].Should().Contain("LIMIT_EXCEEDED");
        _branchRepoMock.Verify(r => r.ToggleIsActiveAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_BranchNotFound_ThrowsBranchNotFoundException()
    {
        var command = CreateValidCommand(999);
        _branchRepoMock.Setup(r => r.GetByIdAsync(999, It.IsAny<CancellationToken>())).ReturnsAsync((Branch?)null);

        var act = () => _handler.Handle(command, CancellationToken.None);
        await act.Should().ThrowAsync<BranchNotFoundException>();
    }

    [Theory]
    [InlineData(1)]
    [InlineData(5)]
    [InlineData(100)]
    public async Task Handle_DifferentBranchIds_ToggleCorrectBranch(int branchId)
    {
        var command = CreateValidCommand(branchId);
        _branchRepoMock.Setup(r => r.GetByIdAsync(branchId, It.IsAny<CancellationToken>())).ReturnsAsync(CreateBranch(branchId, isActive: true));
        _branchRepoMock.Setup(r => r.ToggleIsActiveAsync(branchId, It.IsAny<CancellationToken>())).ReturnsAsync(false);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        _branchRepoMock.Verify(r => r.ToggleIsActiveAsync(branchId, It.IsAny<CancellationToken>()), Times.Once);
    }
}
