using FluentAssertions;
using FluentValidation.TestHelper;
using MediatR;
using Moq;
using SportAcademy.Application.Commands.TraineeGroupCommands.SwitchTraineeGroupToPrivate;
using SportAcademy.Application.Interfaces;
using SportAcademy.Application.Validators.TraineeGroupValidators;
using SportAcademy.Domain.Contract;
using SportAcademy.Domain.Entities;
using SportAcademy.Domain.Enums;
using SportAcademy.Domain.Exceptions.TraineeGroupExceptions;

namespace SportAcademy.Tests.Application.Handlers;

// Switching a public group to private: the chosen trainees stay, everyone else's enrollment is
// ended (freeing them to continue their subscription in another group), and the group becomes
// private at the new, smaller capacity - all in one save.
public class SwitchTraineeGroupToPrivateCommandHandlerTests
{
    private const int GroupId = 21;

    private readonly Mock<ITraineeGroupRepository> _groups = new();
    private readonly Mock<IEnrollmentRepository> _enrollments = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();
    private readonly Mock<IPublisher> _publisher = new();

    private static TraineeGroup PublicGroup() => new()
    {
        Id = GroupId, Name = "G", Type = TraineeGroupType.Public, MaximumCapacity = 15,
    };

    private static Enrollment Enrollment(int id, int traineeId, EnrollmentStatus status = EnrollmentStatus.Active) => new()
    {
        Id = id, TraineeId = traineeId, TraineeGroupId = GroupId, Status = status, SessionRemaining = 5,
    };

    private void Arrange(TraineeGroup group, params Enrollment[] current)
    {
        _groups.Setup(g => g.GetByIdAsync(GroupId, It.IsAny<CancellationToken>())).ReturnsAsync(group);
        _enrollments.Setup(e => e.GetActiveEnrollmentsForGroupAsync(GroupId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(current.ToList());
    }

    private Task<SportAcademy.Application.Common.Result.Result<int>> Switch(int capacity, params int[] keep) =>
        new SwitchTraineeGroupToPrivateCommandHandler(
                _groups.Object, _enrollments.Object, _unitOfWork.Object, _publisher.Object)
            .Handle(new SwitchTraineeGroupToPrivateCommand(GroupId, capacity, keep), CancellationToken.None);

    [Fact]
    public async Task SelectedTraineesStay_TheRestAreEnded_AndTheGroupBecomesPrivate()
    {
        var group = PublicGroup();
        var stays = Enrollment(1, traineeId: 101);
        var leaves = Enrollment(2, traineeId: 102);
        var leavesSuspended = Enrollment(3, traineeId: 103, EnrollmentStatus.Suspended);
        Arrange(group, stays, leaves, leavesSuspended);

        var result = await Switch(capacity: 4, keep: 101);

        result.IsSuccess.Should().BeTrue();
        result.Data.Should().Be(2);

        group.Type.Should().Be(TraineeGroupType.Private);
        group.MaximumCapacity.Should().Be(4);

        stays.Status.Should().Be(EnrollmentStatus.Active);
        stays.EndDate.Should().BeNull();

        foreach (var ended in new[] { leaves, leavesSuspended })
        {
            ended.Status.Should().Be(EnrollmentStatus.Ended);
            ended.EndDate.Should().NotBeNull();
            // Their remaining sessions are kept - that's what continues in the next group.
            ended.SessionRemaining.Should().Be(5);
        }

        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task KeepingNobody_EndsEveryone()
    {
        var group = PublicGroup();
        var a = Enrollment(1, 101);
        var b = Enrollment(2, 102);
        Arrange(group, a, b);

        var result = await Switch(capacity: 3);

        result.Data.Should().Be(2);
        a.Status.Should().Be(EnrollmentStatus.Ended);
        b.Status.Should().Be(EnrollmentStatus.Ended);
        group.Type.Should().Be(TraineeGroupType.Private);
    }

    [Fact]
    public async Task AGroupThatIsAlreadyPrivate_IsRefusedAndNothingChanges()
    {
        var group = PublicGroup();
        group.Type = TraineeGroupType.Private;
        var enrollment = Enrollment(1, 101);
        Arrange(group, enrollment);

        var act = () => Switch(capacity: 4, keep: 101);

        await act.Should().ThrowAsync<GroupAlreadyPrivateException>();
        enrollment.Status.Should().Be(EnrollmentStatus.Active);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task KeepingATraineeWhoIsNotInTheGroup_IsRefusedAndNothingChanges()
    {
        var group = PublicGroup();
        var enrollment = Enrollment(1, 101);
        Arrange(group, enrollment);

        var act = () => Switch(capacity: 4, keep: 999);

        await act.Should().ThrowAsync<KeptTraineeNotInGroupException>();
        enrollment.Status.Should().Be(EnrollmentStatus.Active);
        group.Type.Should().Be(TraineeGroupType.Public);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public void Validator_CapacityAbovePrivateMaximum_IsRejected()
    {
        var validator = new SwitchTraineeGroupToPrivateValidator();

        validator.TestValidate(new SwitchTraineeGroupToPrivateCommand(GroupId, TraineeGroupCapacity.PrivateMaximum + 1, []))
            .ShouldHaveValidationErrorFor(c => c.MaximumCapacity);
    }

    [Fact]
    public void Validator_MoreKeptTraineesThanCapacity_IsRejected()
    {
        var validator = new SwitchTraineeGroupToPrivateValidator();

        validator.TestValidate(new SwitchTraineeGroupToPrivateCommand(GroupId, 2, [1, 2, 3]))
            .IsValid.Should().BeFalse();
    }

    [Fact]
    public void Validator_ValidRequest_Passes()
    {
        var validator = new SwitchTraineeGroupToPrivateValidator();

        validator.TestValidate(new SwitchTraineeGroupToPrivateCommand(GroupId, 4, [1, 2]))
            .ShouldNotHaveAnyValidationErrors();
    }
}
