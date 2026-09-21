using FluentAssertions;
using MediatR;
using Moq;
using SportAcademy.Application.Commands.EnrollmentCommands.ChangeEnrollmentGroup;
using SportAcademy.Application.Commands.EnrollmentCommands.CreateEnrollment;
using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Contract;
using SportAcademy.Domain.Entities;
using SportAcademy.Domain.Enums;
using SportAcademy.Domain.Exceptions.EnrollmentExceptions;

namespace SportAcademy.Tests.Application.Handlers;

// The skill-level rules for putting a trainee into a group (CreateEnrollment, ChangeEnrollmentGroup):
//  - trainee below the group's level  -> allowed, the trainee's level is raised to the group's
//  - trainee above the group's level  -> rejected (the dropdowns never offer it in the first place)
//  - no level on record (row missing or NotSpecified) -> allowed, the group defines the level
// The upgrade must be saved together with the enrollment, never on its own.
public class EnrollmentSkillLevelTests
{
    private const int SportId = 5;
    private const int TraineeId = 11;
    private const int GroupId = 21;

    private readonly Mock<IEnrollmentRepository> _enrollments = new();
    private readonly Mock<ISubscriptionDetailsRepository> _subscriptions = new();
    private readonly Mock<ITraineeGroupRepository> _groups = new();
    private readonly Mock<ITraineeRepository> _trainees = new();
    private readonly Mock<ISportTraineeRepository> _sportTrainees = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();
    private readonly Mock<IPublisher> _publisher = new();

    private static TraineeGroup Group(SkillLevel level) => new()
    {
        Id = GroupId,
        Name = "G",
        SkillLevel = level,
        Type = TraineeGroupType.Public,
        Gender = TraineeGroupGender.Mixed,
        MaximumCapacity = 15,
        IsActive = true,
        GroupSchedules = [new GroupSchedule { Day = DayOfWeek.Monday }, new GroupSchedule { Day = DayOfWeek.Wednesday }],
    };

    private static Trainee NewTrainee() => new()
    {
        Id = TraineeId, Gender = Gender.Male, FirstName = "T", LastName = "T", SSN = "1", PhoneNumber = "1",
    };

    private static SubscriptionDetails Subscription() => new()
    {
        Id = 1,
        StartDate = DateOnly.FromDateTime(DateTime.Today),
        EndDate = DateOnly.FromDateTime(DateTime.Today.AddMonths(1)),
        SportId = SportId,
        GroupType = TraineeGroupType.Public,
        SportPrice = new SportPrice
        {
            SportSubscriptionType = new SportSubscriptionType
            {
                SubscriptionType = new SubscriptionType { DaysPerMonth = 8, NumberOfMonths = 1 },
            },
        },
    };

    private void ArrangeCreate(TraineeGroup group, SportTrainee? existingSkill)
    {
        _groups.Setup(g => g.GetByIdWithSchedulesAsync(GroupId, It.IsAny<CancellationToken>())).ReturnsAsync(group);
        _groups.Setup(g => g.GetSportIdAsync(GroupId, It.IsAny<CancellationToken>())).ReturnsAsync(SportId);
        _enrollments.Setup(e => e.GetActiveEnrollmentCountForGroupAsync(GroupId, It.IsAny<CancellationToken>())).ReturnsAsync(0);
        _subscriptions.Setup(s => s.GetSubscriptionDetailsWithSubTypeAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Subscription());
        _trainees.Setup(t => t.GetFullTrainee(TraineeId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(NewTrainee());
        _sportTrainees.Setup(s => s.GetByIdWithIncludesAsync(SportId, TraineeId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingSkill);
    }

    private Task<SportAcademy.Application.Common.Result.Result<int>> Enroll() =>
        new CreateEnrollmentCommandHandler(
            _enrollments.Object, _subscriptions.Object, _groups.Object, _trainees.Object,
            _sportTrainees.Object, _unitOfWork.Object, _publisher.Object)
        .Handle(new CreateEnrollmentCommand(DateTime.Today, DateTime.Today.AddMonths(1), null, TraineeId, GroupId, 1),
            CancellationToken.None);

    [Fact]
    public async Task Create_TraineeWithNoSkillRow_GetsTheGroupsLevelRecorded()
    {
        ArrangeCreate(Group(SkillLevel.Advanced), existingSkill: null);

        var result = await Enroll();

        result.IsSuccess.Should().BeTrue();
        _sportTrainees.Verify(s => s.AddAsyncWithoutSave(
            It.Is<SportTrainee>(st => st.SportId == SportId && st.TraineeId == TraineeId
                && st.SkillLevel == SkillLevel.Advanced),
            It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Create_TraineeWithNotSpecifiedLevel_IsRaisedToTheGroupsLevel()
    {
        var skill = new SportTrainee { SportId = SportId, TraineeId = TraineeId, SkillLevel = SkillLevel.NotSpecified };
        ArrangeCreate(Group(SkillLevel.Intermediate), skill);

        var result = await Enroll();

        result.IsSuccess.Should().BeTrue();
        skill.SkillLevel.Should().Be(SkillLevel.Intermediate);
    }

    [Fact]
    public async Task Create_TraineeBelowTheGroup_IsUpgradedInTheSameSaveAsTheEnrollment()
    {
        var skill = new SportTrainee { SportId = SportId, TraineeId = TraineeId, SkillLevel = SkillLevel.Beginner };
        ArrangeCreate(Group(SkillLevel.Advanced), skill);

        var result = await Enroll();

        result.IsSuccess.Should().BeTrue();
        skill.SkillLevel.Should().Be(SkillLevel.Advanced);
        // Never persisted on its own ahead of the enrollment.
        _sportTrainees.Verify(s => s.UpdateAsync(It.IsAny<SportTrainee>(), It.IsAny<CancellationToken>()), Times.Never);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Create_TraineeAboveTheGroup_IsRejectedAndNothingIsChanged()
    {
        var skill = new SportTrainee { SportId = SportId, TraineeId = TraineeId, SkillLevel = SkillLevel.Advanced };
        ArrangeCreate(Group(SkillLevel.Beginner), skill);

        var act = () => Enroll();

        await act.Should().ThrowAsync<GroupSkillLevelTooLowException>();
        skill.SkillLevel.Should().Be(SkillLevel.Advanced);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ChangeGroup_TraineeWithNoSkillRow_GetsTheNewGroupsLevelRecorded()
    {
        var group = Group(SkillLevel.Intermediate);
        _enrollments.Setup(e => e.GetByIdWithGroupAndSubscriptionAsync(7, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Enrollment { Id = 7, TraineeId = TraineeId, TraineeGroupId = 99 });
        _groups.Setup(g => g.GetByIdAsync(GroupId, It.IsAny<CancellationToken>())).ReturnsAsync(group);
        _groups.Setup(g => g.GetSportIdAsync(99, It.IsAny<CancellationToken>())).ReturnsAsync(SportId);
        _groups.Setup(g => g.GetSportIdAsync(GroupId, It.IsAny<CancellationToken>())).ReturnsAsync(SportId);
        _trainees.Setup(t => t.GetFullTrainee(TraineeId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(NewTrainee());
        _sportTrainees.Setup(s => s.GetByIdWithIncludesAsync(SportId, TraineeId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((SportTrainee?)null);
        _enrollments.Setup(e => e.GetActiveEnrollmentCountForGroupAsync(GroupId, It.IsAny<CancellationToken>())).ReturnsAsync(0);

        var result = await new ChangeEnrollmentGroupCommandHandler(
                _enrollments.Object, _groups.Object, _trainees.Object, _sportTrainees.Object, _publisher.Object)
            .Handle(new ChangeEnrollmentGroupCommand(7, GroupId), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        _sportTrainees.Verify(s => s.AddAsyncWithoutSave(
            It.Is<SportTrainee>(st => st.SkillLevel == SkillLevel.Intermediate),
            It.IsAny<CancellationToken>()), Times.Once);
    }
}
