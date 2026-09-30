using FluentAssertions;
using MediatR;
using Moq;
using SportAcademy.Application.Commands.EnrollmentCommands.CreateEnrollment;
using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Contract;
using SportAcademy.Domain.Entities;
using SportAcademy.Domain.Enums;
using SportAcademy.Domain.Exceptions.EnrollmentExceptions;
using SportAcademy.Domain.Services;

namespace SportAcademy.Tests.Application.Handlers;

// Enrolling an upcoming subscription into the group the trainee is already training in extends
// the current enrollment's end date; every other second enrollment for the sport is still refused.
public class CreateEnrollmentUpcomingSubscriptionTests
{
    private const int SportId = 5;
    private const int TraineeId = 11;
    private const int GroupId = 21;
    private const int CurrentSubId = 1;
    private const int UpcomingSubId = 2;

    private readonly Mock<IEnrollmentRepository> _enrollments = new();
    private readonly Mock<ISubscriptionDetailsRepository> _subscriptions = new();
    private readonly Mock<ITraineeGroupRepository> _groups = new();
    private readonly Mock<ITraineeRepository> _trainees = new();
    private readonly Mock<ISportTraineeRepository> _sportTrainees = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();
    private readonly Mock<IPublisher> _publisher = new();

    private static readonly DateOnly Today = TenantCalendar.Today;

    private static TraineeGroup Group(int capacity = 15) => new()
    {
        Id = GroupId,
        Name = "G",
        SkillLevel = SkillLevel.Beginner,
        Type = TraineeGroupType.Public,
        Gender = TraineeGroupGender.Mixed,
        MaximumCapacity = capacity,
        IsActive = true,
        GroupSchedules = [new GroupSchedule { Day = DayOfWeek.Monday }, new GroupSchedule { Day = DayOfWeek.Wednesday }],
    };

    private static Enrollment Current(int groupId = GroupId, EnrollmentStatus status = EnrollmentStatus.Active) => new()
    {
        Id = 7,
        TraineeId = TraineeId,
        TraineeGroupId = groupId,
        SubscriptionDetailsId = CurrentSubId,
        Status = status,
        ExpiryDate = Today.AddDays(10).ToDateTime(TimeOnly.MinValue),
    };

    private static SubscriptionDetails Upcoming(int traineeId = TraineeId, int startInDays = 11) => new()
    {
        Id = UpcomingSubId,
        TraineeId = traineeId,
        StartDate = Today.AddDays(startInDays),
        EndDate = Today.AddDays(startInDays + 30),
        SportId = SportId,
        GroupType = TraineeGroupType.Public,
        Status = SubscriptionStatus.Active,
    };

    private void Arrange(Enrollment? existing, SubscriptionDetails sub, int capacity = 15, int activeCount = 1)
    {
        _groups.Setup(g => g.GetByIdWithSchedulesAsync(GroupId, It.IsAny<CancellationToken>())).ReturnsAsync(Group(capacity));
        _groups.Setup(g => g.GetSportIdAsync(GroupId, It.IsAny<CancellationToken>())).ReturnsAsync(SportId);
        _enrollments.Setup(e => e.GetActiveEnrollmentCountForGroupAsync(GroupId, It.IsAny<CancellationToken>())).ReturnsAsync(activeCount);
        _enrollments.Setup(e => e.GetCurrentEnrollmentForSportAsync(TraineeId, SportId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existing);
        _subscriptions.Setup(s => s.GetSubscriptionDetailsWithSubTypeAsync(UpcomingSubId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(sub);
    }

    private Task<SportAcademy.Application.Common.Result.Result<int>> Enroll() =>
        new CreateEnrollmentCommandHandler(
            _enrollments.Object, _subscriptions.Object, _groups.Object, _trainees.Object,
            _sportTrainees.Object, _unitOfWork.Object, _publisher.Object)
        .Handle(new CreateEnrollmentCommand(DateTime.Today, DateTime.Today.AddMonths(1), null, TraineeId, GroupId, UpcomingSubId),
            CancellationToken.None);

    [Fact]
    public async Task UpcomingSubscription_SameGroup_ExtendsTheEnrollmentEndDate()
    {
        var existing = Current();
        var upcoming = Upcoming();
        Arrange(existing, upcoming);

        var result = await Enroll();

        result.IsSuccess.Should().BeTrue();
        result.Data.Should().Be(existing.Id);
        existing.ExpiryDate.Should().Be(upcoming.EndDate.ToDateTime(TimeOnly.MinValue));
        // Still training on the subscription it is on now; the lifecycle service hands over later.
        existing.SubscriptionDetailsId.Should().Be(CurrentSubId);
        _enrollments.Verify(e => e.UpdateAsync(existing, It.IsAny<CancellationToken>()), Times.Once);
        _enrollments.Verify(e => e.AddAsyncWithoutSave(It.IsAny<Enrollment>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task UpcomingSubscription_InAFullGroup_StillExtends_BecauseTheTraineeAlreadyHoldsTheSeat()
    {
        var existing = Current();
        Arrange(existing, Upcoming(), capacity: 1, activeCount: 1);

        var result = await Enroll();

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task UpcomingSubscription_EndingBeforeTheCurrentExpiry_NeverShortensIt()
    {
        var existing = Current();
        var originalExpiry = existing.ExpiryDate;
        var upcoming = Upcoming(startInDays: 1);
        upcoming.EndDate = Today.AddDays(5);
        Arrange(existing, upcoming);

        var result = await Enroll();

        result.IsSuccess.Should().BeTrue();
        existing.ExpiryDate.Should().Be(originalExpiry);
        _enrollments.Verify(e => e.UpdateAsync(It.IsAny<Enrollment>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task UpcomingSubscription_DifferentGroup_IsStillRefused()
    {
        Arrange(Current(groupId: 99), Upcoming());

        var act = () => Enroll();

        await act.Should().ThrowAsync<TraineeAlreadyEnrolledInSportException>();
    }

    [Fact]
    public async Task UpcomingSubscription_BelongingToAnotherTrainee_IsStillRefused()
    {
        Arrange(Current(), Upcoming(traineeId: 999));

        var act = () => Enroll();

        await act.Should().ThrowAsync<TraineeAlreadyEnrolledInSportException>();
    }

    [Fact]
    public async Task AlreadyStartedSubscription_SameGroup_IsStillRefused()
    {
        Arrange(Current(), Upcoming(startInDays: 0));

        var act = () => Enroll();

        await act.Should().ThrowAsync<TraineeAlreadyEnrolledInSportException>();
    }

    [Fact]
    public async Task UpcomingSubscription_ForASuspendedEnrollment_IsStillRefusedWithTheSuspendedError()
    {
        Arrange(Current(status: EnrollmentStatus.Suspended), Upcoming());

        var act = () => Enroll();

        await act.Should().ThrowAsync<TraineeHasSuspendedEnrollmentException>();
    }
}
