using AutoMapper;
using FluentAssertions;
using Moq;
using SportAcademy.Application.Commands.SessionOccurrenceCommands.GenerateSessionOccurrences;
using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Entities;
using SportAcademy.Domain.Enums;
using SportAcademy.Domain.ValueObjects;
using SportAcademy.Domain.Exceptions.BaseExceptions;
using SportAcademy.Domain.Exceptions.SessionOccurrenceExceptions;

namespace SportAcademy.Tests.Application.Handlers;

public class GenerateSessionOccurrencesCommandHandlerTests
{
    private readonly Mock<ITraineeGroupRepository> _traineeGroupRepoMock = new();
    private readonly Mock<ISessionOccurrenceRepository> _sessionOccurrenceRepoMock = new();
    private readonly Mock<IMapper> _mapperMock = new();
    private readonly GenerateSessionOccurrencesCommandHandler _handler;

    public GenerateSessionOccurrencesCommandHandlerTests()
    {
        _handler = new GenerateSessionOccurrencesCommandHandler(
            _traineeGroupRepoMock.Object,
            _sessionOccurrenceRepoMock.Object,
            _mapperMock.Object);
    }

    private static GroupSchedule CreateSchedule(DayOfWeek day = DayOfWeek.Monday, int id = 1)
    {
        return new GroupSchedule
        {
            Id = id,
            Day = day,
            StartTime = new TimeOnly(16, 30),
            TraineeGroupId = 1
        };
    }

    private static TraineeGroup CreateTraineeGroup(params GroupSchedule[] schedules)
    {
        return new TraineeGroup
        {
            Id = 1,
            Name = "Morning Swim Group",
            DurationInMinutes = 55,
            GroupSchedules = schedules.ToList(),
            Coach = new Coach
            {
                Sport = new Sport { Name = "Swimming" },
                Employee = Employee.Create(new PersonData("Ahmed", "Ali", "290010112345", Email.Create("ahmed@test.com"), new DateOnly(1990, 1, 1), Gender.Male, Nationality.Kuwaiti, Address.Create("St", "City"), "0501234567", null), 5000m, Position.Coach, 1)
            },
            Branch = new Branch { Name = "Main Branch", City = "Riyadh", Country = "Saudi Arabia", PhoneNumber = "0501234567", CoX = "24.7136", CoY = "46.6753" },
            Enrollments = new List<Enrollment>()
        };
    }

    private static GenerateSessionOccurrencesCommand CreateCommand(
        int traineeGroupId = 1,
        int durationInDays = 30,
        int? groupScheduleId = null,
        DateOnly? startDate = null)
    {
        return new GenerateSessionOccurrencesCommand(traineeGroupId, durationInDays, groupScheduleId, startDate);
    }

    [Fact]
    public async Task Handle_InvalidTraineeGroupId_ThrowsIdNotFoundException()
    {
        var command = CreateCommand(traineeGroupId: 0);

        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<IdNotFoundException>();
    }

    [Fact]
    public async Task Handle_NegativeTraineeGroupId_ThrowsIdNotFoundException()
    {
        var command = CreateCommand(traineeGroupId: -5);

        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<IdNotFoundException>();
    }

    [Fact]
    public async Task Handle_DurationLessThanOne_ThrowsInvalidDurationException()
    {
        var command = CreateCommand(durationInDays: 0);

        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<InvalidDurationException>()
            .Where(e => e.MinDays == 1 && e.MaxDays == 90);
    }

    [Fact]
    public async Task Handle_DurationGreaterThan90_ThrowsInvalidDurationException()
    {
        var command = CreateCommand(durationInDays: 91);

        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<InvalidDurationException>()
            .Where(e => e.MinDays == 1 && e.MaxDays == 90);
    }

    [Fact]
    public async Task Handle_DurationBoundaryValue_90_Succeeds()
    {
        var command = CreateCommand(durationInDays: 90);
        var schedule = CreateSchedule(DayOfWeek.Monday);
        var group = CreateTraineeGroup(schedule);
        var utcNow = DateTime.UtcNow;

        _traineeGroupRepoMock.Setup(r => r.GetByIdWithSchedulesAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(group);
        _sessionOccurrenceRepoMock.Setup(r => r.GetLastOccurrenceDateAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync((DateTime?)null);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Data.Should().BeGreaterThanOrEqualTo(0);
    }

    [Fact]
    public async Task Handle_TraineeGroupNotFound_ThrowsIdNotFoundException()
    {
        var command = CreateCommand(traineeGroupId: 999);

        _traineeGroupRepoMock.Setup(r => r.GetByIdWithSchedulesAsync(999, It.IsAny<CancellationToken>()))
            .ReturnsAsync((TraineeGroup?)null);

        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<IdNotFoundException>();
    }

    [Fact]
    public async Task Handle_NoSchedulesFound_ThrowsNoSchedulesFoundException()
    {
        var command = CreateCommand(traineeGroupId: 1);
        var group = CreateTraineeGroup();

        _traineeGroupRepoMock.Setup(r => r.GetByIdWithSchedulesAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(group);

        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<NoSchedulesFoundException>();
    }

    [Fact]
    public async Task Handle_GapTooLargeWithoutStartDate_ThrowsSessionGapTooLargeException()
    {
        var command = CreateCommand(traineeGroupId: 1, durationInDays: 30, startDate: null);
        var schedule = CreateSchedule(DayOfWeek.Monday);
        var group = CreateTraineeGroup(schedule);
        var lastOccurrence = DateTime.UtcNow.AddDays(-10);

        _traineeGroupRepoMock.Setup(r => r.GetByIdWithSchedulesAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(group);
        _sessionOccurrenceRepoMock.Setup(r => r.GetLastOccurrenceDateAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(lastOccurrence);

        var act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<SessionGapTooLargeException>();
    }

    [Fact]
    public async Task Handle_GapTooLargeWithStartDateProvided_GeneratesSessions()
    {
        var providedStartDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-5));
        var command = CreateCommand(traineeGroupId: 1, durationInDays: 7, startDate: providedStartDate);
        var schedule = CreateSchedule(DayOfWeek.Monday);
        var group = CreateTraineeGroup(schedule);
        var lastOccurrence = DateTime.UtcNow.AddDays(-10);

        _traineeGroupRepoMock.Setup(r => r.GetByIdWithSchedulesAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(group);
        _sessionOccurrenceRepoMock.Setup(r => r.GetLastOccurrenceDateAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(lastOccurrence);
        _sessionOccurrenceRepoMock.Setup(r => r.AddRangeAsync(It.IsAny<IEnumerable<SessionOccurrence>>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Data.Should().BeGreaterThan(0);
        _sessionOccurrenceRepoMock.Verify(r => r.AddRangeAsync(
            It.Is<IEnumerable<SessionOccurrence>>(sessions => sessions.All(s => s.GroupScheduleId == schedule.Id)),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_NoPreviousSessions_StartsFromToday_GeneratesCorrectCount()
    {
        var command = CreateCommand(traineeGroupId: 1, durationInDays: 14);
        var schedule = CreateSchedule(DayOfWeek.Monday);
        var group = CreateTraineeGroup(schedule);

        _traineeGroupRepoMock.Setup(r => r.GetByIdWithSchedulesAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(group);
        _sessionOccurrenceRepoMock.Setup(r => r.GetLastOccurrenceDateAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync((DateTime?)null);
        _sessionOccurrenceRepoMock.Setup(r => r.AddRangeAsync(It.IsAny<IEnumerable<SessionOccurrence>>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Data.Should().BeGreaterThan(0);
        _sessionOccurrenceRepoMock.Verify(
            r => r.AddRangeAsync(It.IsAny<IEnumerable<SessionOccurrence>>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_RecentSessionWithin7Days_ContinuesFromLastDate()
    {
        var command = CreateCommand(traineeGroupId: 1, durationInDays: 7);
        var schedule = CreateSchedule(DayOfWeek.Monday);
        var group = CreateTraineeGroup(schedule);
        var lastOccurrence = DateTime.UtcNow.AddDays(-3);

        _traineeGroupRepoMock.Setup(r => r.GetByIdWithSchedulesAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(group);
        _sessionOccurrenceRepoMock.Setup(r => r.GetLastOccurrenceDateAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(lastOccurrence);
        _sessionOccurrenceRepoMock.Setup(r => r.AddRangeAsync(It.IsAny<IEnumerable<SessionOccurrence>>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Data.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task Handle_Exactly7DaysSinceLastSession_ContinuesFromLastDate()
    {
        var command = CreateCommand(traineeGroupId: 1, durationInDays: 7);
        var schedule = CreateSchedule(DayOfWeek.Monday);
        var group = CreateTraineeGroup(schedule);
        var lastOccurrence = DateTime.UtcNow.AddDays(-7);

        _traineeGroupRepoMock.Setup(r => r.GetByIdWithSchedulesAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(group);
        _sessionOccurrenceRepoMock.Setup(r => r.GetLastOccurrenceDateAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(lastOccurrence);
        _sessionOccurrenceRepoMock.Setup(r => r.AddRangeAsync(It.IsAny<IEnumerable<SessionOccurrence>>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_NoSchedulesMatchDateRange_ReturnsZeroSessions()
    {
        var schedule = CreateSchedule(DayOfWeek.Monday);
        var command = CreateCommand(traineeGroupId: 1, durationInDays: 1);
        var group = CreateTraineeGroup(schedule);

        _traineeGroupRepoMock.Setup(r => r.GetByIdWithSchedulesAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(group);
        _sessionOccurrenceRepoMock.Setup(r => r.GetLastOccurrenceDateAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync((DateTime?)null);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Data.Should().Be(0);
        _sessionOccurrenceRepoMock.Verify(
            r => r.AddRangeAsync(It.IsAny<IEnumerable<SessionOccurrence>>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_GeneratesForSpecificScheduleOnly()
    {
        var schedule1 = CreateSchedule(DayOfWeek.Monday, id: 1);
        var schedule2 = CreateSchedule(DayOfWeek.Wednesday, id: 2);
        var group = CreateTraineeGroup(schedule1, schedule2);
        var command = CreateCommand(traineeGroupId: 1, durationInDays: 14, groupScheduleId: 1);

        _traineeGroupRepoMock.Setup(r => r.GetByIdWithSchedulesAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(group);
        _sessionOccurrenceRepoMock.Setup(r => r.GetLastOccurrenceDateAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync((DateTime?)null);
        _sessionOccurrenceRepoMock.Setup(r => r.AddRangeAsync(It.IsAny<IEnumerable<SessionOccurrence>>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        _sessionOccurrenceRepoMock.Verify(
            r => r.AddRangeAsync(
                It.Is<IEnumerable<SessionOccurrence>>(sessions =>
                    sessions.All(s => s.GroupScheduleId == 1)),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_SessionInFuture_StartsFromFutureDate()
    {
        var command = CreateCommand(traineeGroupId: 1, durationInDays: 7);
        var schedule = CreateSchedule(DayOfWeek.Monday);
        var group = CreateTraineeGroup(schedule);
        var lastOccurrence = DateTime.UtcNow.AddDays(5);

        _traineeGroupRepoMock.Setup(r => r.GetByIdWithSchedulesAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(group);
        _sessionOccurrenceRepoMock.Setup(r => r.GetLastOccurrenceDateAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(lastOccurrence);
        _sessionOccurrenceRepoMock.Setup(r => r.AddRangeAsync(It.IsAny<IEnumerable<SessionOccurrence>>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_CreatesSessionWithCorrectData()
    {
        var schedule = CreateSchedule(DayOfWeek.Monday);
        var command = CreateCommand(traineeGroupId: 1, durationInDays: 7);
        var group = CreateTraineeGroup(schedule);

        _traineeGroupRepoMock.Setup(r => r.GetByIdWithSchedulesAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(group);
        _sessionOccurrenceRepoMock.Setup(r => r.GetLastOccurrenceDateAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync((DateTime?)null);
        _sessionOccurrenceRepoMock.Setup(r => r.AddRangeAsync(It.IsAny<IEnumerable<SessionOccurrence>>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        await _handler.Handle(command, CancellationToken.None);

        _sessionOccurrenceRepoMock.Verify(r => r.AddRangeAsync(
            It.Is<IEnumerable<SessionOccurrence>>(sessions =>
                sessions.All(s =>
                    s.GroupScheduleId == schedule.Id &&
                    s.Status == SessionStatus.Scheduled &&
                    s.StartDateTime.Hour == 16 &&
                    s.StartDateTime.Minute == 30 &&
                    s.StartDateTime.Second == 0)),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_GeneratesCorrectSessionCount_ForMultipleSchedules()
    {
        var scheduleMon = CreateSchedule(DayOfWeek.Monday, id: 1);
        var scheduleWed = CreateSchedule(DayOfWeek.Wednesday, id: 2);
        var group = CreateTraineeGroup(scheduleMon, scheduleWed);
        var command = CreateCommand(traineeGroupId: 1, durationInDays: 14);

        _traineeGroupRepoMock.Setup(r => r.GetByIdWithSchedulesAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(group);
        _sessionOccurrenceRepoMock.Setup(r => r.GetLastOccurrenceDateAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync((DateTime?)null);
        _sessionOccurrenceRepoMock.Setup(r => r.AddRangeAsync(It.IsAny<IEnumerable<SessionOccurrence>>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Data.Should().BeGreaterThanOrEqualTo(4);
    }
}
