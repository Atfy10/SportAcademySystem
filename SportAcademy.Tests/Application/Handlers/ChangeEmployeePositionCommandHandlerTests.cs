using FluentAssertions;
using MediatR;
using Moq;
using SportAcademy.Application.Commands.EmployeeCommands.ChangeEmployeePosition;
using SportAcademy.Application.Events;
using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Contract;
using SportAcademy.Domain.Entities;
using SportAcademy.Domain.Enums;
using SportAcademy.Domain.Exceptions.EmployeeExceptions;
using SportAcademy.Domain.Services;
using SportAcademy.Domain.ValueObjects;

namespace SportAcademy.Tests.Application.Handlers;

public class ChangeEmployeePositionCommandHandlerTests
{
    private readonly Mock<IEmployeeRepository> _employeeRepoMock = new();
    private readonly Mock<ICoachRepository> _coachRepoMock = new();
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly Mock<IUserContextService> _userContextMock = new();
    private readonly Mock<IUserRepository> _userRepoMock = new();
    private readonly Mock<IPublisher> _publisherMock = new();
    private readonly ChangeEmployeePositionCommandHandler _handler;

    public ChangeEmployeePositionCommandHandlerTests()
    {
        _handler = new ChangeEmployeePositionCommandHandler(
            _employeeRepoMock.Object, _coachRepoMock.Object, _unitOfWorkMock.Object,
            _userContextMock.Object, _userRepoMock.Object, _publisherMock.Object);
    }

    private Employee SetUpEmployee(Position position)
    {
        var employee = new Employee
        {
            Id = 1,
            FirstName = "Sara",
            LastName = "Ahmed",
            SSN = "123456789",
            Email = Email.Create("sara@example.com"),
            Address = Address.Create("Street", "City"),
            PhoneNumber = "+96551234567",
            BranchId = 1,
            Position = position,
        };
        _employeeRepoMock.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(employee);
        return employee;
    }

    private Employee SetUpCoachEmployee(bool hasCoachRecord = true, CoachRemovalBlockers? blockers = null)
    {
        var employee = SetUpEmployee(Position.Coach);
        _coachRepoMock
            .Setup(r => r.GetByEmployeeIdIncludingDeletedAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(hasCoachRecord ? new Coach { EmployeeId = 1 } : null);
        _coachRepoMock
            .Setup(r => r.GetRemovalBlockersAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(blockers ?? new CoachRemovalBlockers(0, [], 0));
        return employee;
    }

    [Fact]
    public async Task Handle_CoachMovedToAnotherPosition_WithNoBlockers_HardDeletesCoachRecordAndChangesPosition()
    {
        var employee = SetUpCoachEmployee();

        var result = await _handler.Handle(new ChangeEmployeePositionCommand(1, Position.HR), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        employee.Position.Should().Be(Position.HR);
        _coachRepoMock.Verify(r => r.HardDeleteAsync(1, It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        _publisherMock.Verify(p => p.Publish(
            It.Is<EmployeeLifecycleEvent>(e => e.EmployeeId == 1 && e.Action == "Removed as Coach"),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_CoachStillHasGroups_ThrowsWithGroupNamesAndChangesNothing()
    {
        var employee = SetUpCoachEmployee(blockers: new CoachRemovalBlockers(2, ["Juniors A", "Seniors B"], 0));

        var act = () => _handler.Handle(new ChangeEmployeePositionCommand(1, Position.HR), CancellationToken.None);

        var ex = (await act.Should().ThrowAsync<CoachHasGroupsException>()).Which;
        ex.Message.Should().Contain("Juniors A").And.Contain("Seniors B");
        employee.Position.Should().Be(Position.Coach);
        _coachRepoMock.Verify(r => r.HardDeleteAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_CoachHasManyGroups_ListsOnlyTheFirstFiveNames()
    {
        SetUpCoachEmployee(blockers: new CoachRemovalBlockers(7, ["G1", "G2", "G3", "G4", "G5", "G6", "G7"], 0));

        var act = () => _handler.Handle(new ChangeEmployeePositionCommand(1, Position.HR), CancellationToken.None);

        var ex = (await act.Should().ThrowAsync<CoachHasGroupsException>()).Which;
        ex.Message.Should().Contain("G5").And.Contain("...").And.NotContain("G6");
    }

    [Fact]
    public async Task Handle_CoachHasTrainingHistory_ThrowsAndChangesNothing()
    {
        var employee = SetUpCoachEmployee(blockers: new CoachRemovalBlockers(0, [], 4));

        var act = () => _handler.Handle(new ChangeEmployeePositionCommand(1, Position.Manager), CancellationToken.None);

        await act.Should().ThrowAsync<CoachHasTrainingHistoryException>();
        employee.Position.Should().Be(Position.Coach);
        _coachRepoMock.Verify(r => r.HardDeleteAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_CoachPositionButNoCoachRecord_ChangesPositionWithoutDeleting()
    {
        var employee = SetUpCoachEmployee(hasCoachRecord: false);

        var result = await _handler.Handle(new ChangeEmployeePositionCommand(1, Position.HR), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        employee.Position.Should().Be(Position.HR);
        _coachRepoMock.Verify(r => r.GetRemovalBlockersAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
        _coachRepoMock.Verify(r => r.HardDeleteAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_SamePosition_IsANoOp()
    {
        SetUpCoachEmployee();

        var result = await _handler.Handle(new ChangeEmployeePositionCommand(1, Position.Coach), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        _coachRepoMock.Verify(r => r.GetRemovalBlockersAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
        _coachRepoMock.Verify(r => r.HardDeleteAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_NonCoachChangesPosition_DoesNotTouchCoachRecord()
    {
        var employee = SetUpEmployee(Position.Accountant);

        var result = await _handler.Handle(new ChangeEmployeePositionCommand(1, Position.HR), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        employee.Position.Should().Be(Position.HR);
        _coachRepoMock.Verify(r => r.GetByEmployeeIdIncludingDeletedAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
        _coachRepoMock.Verify(r => r.HardDeleteAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_EmployeeNotFound_Throws()
    {
        _employeeRepoMock.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync((Employee?)null);

        var act = () => _handler.Handle(new ChangeEmployeePositionCommand(1, Position.HR), CancellationToken.None);

        await act.Should().ThrowAsync<EmployeeNotFoundException>();
    }
}
