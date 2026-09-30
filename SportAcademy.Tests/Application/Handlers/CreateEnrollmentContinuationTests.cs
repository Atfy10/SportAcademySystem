using FluentAssertions;
using MediatR;
using Moq;
using SportAcademy.Application.Commands.EnrollmentCommands.CreateEnrollment;
using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Contract;
using SportAcademy.Domain.Entities;
using SportAcademy.Domain.Enums;

namespace SportAcademy.Tests.Application.Handlers;

// A trainee whose enrollment was ended while their subscription still had sessions left (their
// group went private and they didn't carry on) continues that subscription in another group: the
// new enrollment gets only the sessions still owed, not a fresh full allowance.
public class CreateEnrollmentContinuationTests
{
    private const int SportId = 5;
    private const int TraineeId = 11;
    private const int GroupId = 21;
    private const int SubId = 1;

    private readonly Mock<IEnrollmentRepository> _enrollments = new();
    private readonly Mock<ISubscriptionDetailsRepository> _subscriptions = new();
    private readonly Mock<ITraineeGroupRepository> _groups = new();
    private readonly Mock<ITraineeRepository> _trainees = new();
    private readonly Mock<ISportTraineeRepository> _sportTrainees = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();
    private readonly Mock<IPublisher> _publisher = new();

    private Enrollment? _added;

    private void Arrange(Enrollment? previous)
    {
        _groups.Setup(g => g.GetByIdWithSchedulesAsync(GroupId, It.IsAny<CancellationToken>())).ReturnsAsync(new TraineeGroup
        {
            Id = GroupId, Name = "G", SkillLevel = SkillLevel.Beginner, Type = TraineeGroupType.Public,
            Gender = TraineeGroupGender.Mixed, MaximumCapacity = 15, IsActive = true,
            GroupSchedules = [new GroupSchedule { Day = DayOfWeek.Monday }, new GroupSchedule { Day = DayOfWeek.Wednesday }],
        });
        _groups.Setup(g => g.GetSportIdAsync(GroupId, It.IsAny<CancellationToken>())).ReturnsAsync(SportId);
        _subscriptions.Setup(s => s.GetSubscriptionDetailsWithSubTypeAsync(SubId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new SubscriptionDetails
            {
                Id = SubId, TraineeId = TraineeId, SportId = SportId, GroupType = TraineeGroupType.Public,
                StartDate = DateOnly.FromDateTime(DateTime.Today.AddDays(-10)),
                EndDate = DateOnly.FromDateTime(DateTime.Today.AddMonths(1)),
                SportPrice = new SportPrice
                {
                    SportSubscriptionType = new SportSubscriptionType
                    {
                        SubscriptionType = new SubscriptionType { DaysPerMonth = 8, NumberOfMonths = 1 },
                    },
                },
            });
        _trainees.Setup(t => t.GetFullTrainee(TraineeId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Trainee { Id = TraineeId, Gender = Gender.Male, FirstName = "T", LastName = "T", SSN = "1", PhoneNumber = "1" });
        _enrollments.Setup(e => e.GetLatestEndedForSubscriptionAsync(SubId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(previous);
        _enrollments.Setup(e => e.AddAsyncWithoutSave(It.IsAny<Enrollment>(), It.IsAny<CancellationToken>()))
            .Callback<Enrollment, CancellationToken>((e, _) => _added = e)
            .ReturnsAsync((Enrollment e, CancellationToken _) => e);
    }

    private Task<SportAcademy.Application.Common.Result.Result<int>> Enroll() =>
        new CreateEnrollmentCommandHandler(
            _enrollments.Object, _subscriptions.Object, _groups.Object, _trainees.Object,
            _sportTrainees.Object, _unitOfWork.Object, _publisher.Object)
        .Handle(new CreateEnrollmentCommand(DateTime.Today, DateTime.Today.AddMonths(1), null, TraineeId, GroupId, SubId),
            CancellationToken.None);

    [Fact]
    public async Task SubscriptionWithAnEndedEnrollmentThatHasSessionsLeft_IsContinuedWithOnlyThoseSessions()
    {
        Arrange(new Enrollment
        {
            Id = 7, TraineeId = TraineeId, SubscriptionDetailsId = SubId,
            Status = EnrollmentStatus.Ended, EndDate = DateTime.UtcNow, SessionAllowed = 8, SessionRemaining = 3,
        });

        var result = await Enroll();

        result.IsSuccess.Should().BeTrue();
        _added.Should().NotBeNull();
        _added!.SessionAllowed.Should().Be(3);
        _added.SessionRemaining.Should().Be(3);
        // Three Monday/Wednesday sessions run out within about two weeks - well short of the
        // month a fresh 8-session allowance would take.
        _added.ExpiryDate.Should().BeBefore(DateTime.Today.AddDays(15));
    }

    [Fact]
    public async Task SubscriptionNeverUsedBefore_GetsItsFullAllowance()
    {
        Arrange(previous: null);

        var result = await Enroll();

        result.IsSuccess.Should().BeTrue();
        _added!.SessionAllowed.Should().Be(8);
        _added.SessionRemaining.Should().Be(8);
    }
}
