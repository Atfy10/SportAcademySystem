using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using SportAcademy.Application.Behaviors;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Entities;

namespace SportAcademy.Tests.Application.Behaviors;

// Regression coverage for a real bug found by manual QA: after the plan-limits downgrade wizard
// deactivates a branch or sport, the tenant could still create/reassign other records against it
// as if nothing had changed - none of the affected create/update handlers ever checked
// Branch.IsActive/Sport.IsActive at all (a pre-existing gap, not specific to the wizard). This
// behavior is the shared fix, mirroring BranchAccessValidationBehavior/LimitGateBehavior's own
// "one opt-in marker interface, one pipeline behavior" shape.
public class ActiveResourceGuardBehaviorTests
{
    private readonly Mock<IBranchRepository> _branchRepoMock = new();
    private readonly Mock<ISportRepository> _sportRepoMock = new();
    private readonly ActiveResourceGuardBehavior<object, Result> _behavior;
    private bool _nextWasCalled;

    public ActiveResourceGuardBehaviorTests()
    {
        _behavior = new ActiveResourceGuardBehavior<object, Result>(
            _branchRepoMock.Object, _sportRepoMock.Object,
            Mock.Of<ILogger<ActiveResourceGuardBehavior<object, Result>>>());
    }

    private Task<Result> Next(CancellationToken _)
    {
        _nextWasCalled = true;
        return Task.FromResult(Result.Success("Test"));
    }

    private static Branch MakeBranch(int id, bool isActive) =>
        new() { Id = id, IsActive = isActive, Name = "B", City = "C", Country = "C", PhoneNumber = "0" };

    private static Sport MakeSport(int id, bool isActive) => new() { Id = id, IsActive = isActive, Name = "S" };

    [Fact]
    public async Task Handle_RequiresActiveBranch_BranchIsInactive_RejectsWithoutCallingNext()
    {
        _branchRepoMock.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(MakeBranch(1, isActive: false));

        var result = await _behavior.Handle(new RequiredBranchRequest(1), Next, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Message.Should().Contain("deactivated");
        _nextWasCalled.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_RequiresActiveBranch_BranchIsActive_CallsNext()
    {
        _branchRepoMock.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(MakeBranch(1, isActive: true));

        var result = await _behavior.Handle(new RequiredBranchRequest(1), Next, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        _nextWasCalled.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_RequiresActiveBranch_BranchDoesNotExist_RejectsWithoutCallingNext()
    {
        _branchRepoMock.Setup(r => r.GetByIdAsync(999, It.IsAny<CancellationToken>())).ReturnsAsync((Branch?)null);

        var result = await _behavior.Handle(new RequiredBranchRequest(999), Next, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        _nextWasCalled.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_OptionalBranchId_Null_SkipsCheckEntirely()
    {
        var result = await _behavior.Handle(new OptionalBranchRequest(null), Next, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        _nextWasCalled.Should().BeTrue();
        _branchRepoMock.Verify(r => r.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_OptionalBranchId_SetAndInactive_RejectsWithoutCallingNext()
    {
        _branchRepoMock.Setup(r => r.GetByIdAsync(2, It.IsAny<CancellationToken>())).ReturnsAsync(MakeBranch(2, isActive: false));

        var result = await _behavior.Handle(new OptionalBranchRequest(2), Next, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        _nextWasCalled.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_RequiresActiveSport_SportIsInactive_RejectsWithoutCallingNext()
    {
        _sportRepoMock.Setup(r => r.GetByIdAsync(5, It.IsAny<CancellationToken>())).ReturnsAsync(MakeSport(5, isActive: false));

        var result = await _behavior.Handle(new RequiredSportRequest(5), Next, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Message.Should().Contain("deactivated");
        _nextWasCalled.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_RequiresActiveSports_OneOfManyIsInactive_RejectsWithoutCallingNext()
    {
        _sportRepoMock.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(MakeSport(1, isActive: true));
        _sportRepoMock.Setup(r => r.GetByIdAsync(2, It.IsAny<CancellationToken>())).ReturnsAsync(MakeSport(2, isActive: false));

        var result = await _behavior.Handle(new SportsRequest([1, 2]), Next, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        _nextWasCalled.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_RequiresActiveSports_Null_SkipsCheckEntirely()
    {
        var result = await _behavior.Handle(new SportsRequest(null), Next, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        _nextWasCalled.Should().BeTrue();
        _sportRepoMock.Verify(r => r.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_RequestImplementsNeitherInterface_CallsNext_TouchesNoRepository()
    {
        var result = await _behavior.Handle(new object(), Next, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        _nextWasCalled.Should().BeTrue();
        _branchRepoMock.Verify(r => r.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
        _sportRepoMock.Verify(r => r.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    // One minimal request type per interface, matching how a real command only ever implements
    // one of the required/optional variants for a given resource - never both (confirmed across
    // every command touched by this fix: e.g. CreateEmployeeCommand implements
    // IRequiresActiveBranch, UpdateEmployeeCommand implements IRequiresActiveOptionalBranch, never
    // the same command implementing both).
    private sealed record RequiredBranchRequest(int BranchId) : IRequiresActiveBranch;
    private sealed record OptionalBranchRequest(int? BranchId) : IRequiresActiveOptionalBranch;
    private sealed record RequiredSportRequest(int SportId) : IRequiresActiveSport;
    private sealed record SportsRequest(IEnumerable<int>? SportIds) : IRequiresActiveSports;
}
