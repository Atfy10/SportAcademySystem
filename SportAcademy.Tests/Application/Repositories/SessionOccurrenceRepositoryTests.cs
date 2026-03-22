using AutoMapper;
using FluentAssertions;
using Moq;
using SportAcademy.Application.Commands.SessionOccurrenceCommands.GenerateSessionOccurrences;
using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Entities;
using SportAcademy.Domain.Enums;

namespace SportAcademy.Tests.Application.Repositories;

public class SessionOccurrenceRepositoryTests
{
    private readonly Mock<ITraineeGroupRepository> _traineeGroupRepoMock = new();
    private readonly Mock<ISessionOccurrenceRepository> _sessionOccurrenceRepoMock = new();
    private readonly Mock<IMapper> _mapperMock = new();

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
                Employee = new Employee { FirstName = "Ahmed", LastName = "Ali", SSN = "123456789012", PhoneNumber = "0501234567" }
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
    public async Task GetLastOccurrenceDateAsync_IsCalled_WithCorrectTraineeGroupId()
    {
        var handler = new GenerateSessionOccurrencesCommandHandler(
            _traineeGroupRepoMock.Object,
            _sessionOccurrenceRepoMock.Object,
            _mapperMock.Object);
        var command = CreateCommand(traineeGroupId: 3);
        var group = CreateTraineeGroup(CreateSchedule());

        _traineeGroupRepoMock.Setup(r => r.GetByIdWithSchedulesAsync(3, It.IsAny<CancellationToken>()))
            .ReturnsAsync(group);
        _sessionOccurrenceRepoMock.Setup(r => r.GetLastOccurrenceDateAsync(3, It.IsAny<CancellationToken>()))
            .ReturnsAsync((DateTime?)null);
        _sessionOccurrenceRepoMock.Setup(r => r.AddRangeAsync(It.IsAny<IEnumerable<SessionOccurrence>>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        await handler.Handle(command, CancellationToken.None);

        _sessionOccurrenceRepoMock.Verify(
            r => r.GetLastOccurrenceDateAsync(3, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task AddRangeAsync_IsCalledWithCorrectEntities()
    {
        var handler = new GenerateSessionOccurrencesCommandHandler(
            _traineeGroupRepoMock.Object,
            _sessionOccurrenceRepoMock.Object,
            _mapperMock.Object);
        var command = CreateCommand(traineeGroupId: 1, durationInDays: 7);
        var schedule = CreateSchedule(DayOfWeek.Monday);
        var group = CreateTraineeGroup(schedule);
        IEnumerable<SessionOccurrence>? capturedSessions = null;

        _traineeGroupRepoMock.Setup(r => r.GetByIdWithSchedulesAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(group);
        _sessionOccurrenceRepoMock.Setup(r => r.GetLastOccurrenceDateAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync((DateTime?)null);
        _sessionOccurrenceRepoMock
            .Setup(r => r.AddRangeAsync(It.IsAny<IEnumerable<SessionOccurrence>>(), It.IsAny<CancellationToken>()))
            .Callback<IEnumerable<SessionOccurrence>, CancellationToken>((sessions, _) => capturedSessions = sessions)
            .Returns(Task.CompletedTask);

        await handler.Handle(command, CancellationToken.None);

        _sessionOccurrenceRepoMock.Verify(
            r => r.AddRangeAsync(It.IsAny<IEnumerable<SessionOccurrence>>(), It.IsAny<CancellationToken>()),
            Times.Once);
        capturedSessions.Should().NotBeNull();
        capturedSessions.Should().OnlyContain(s =>
            s.GroupScheduleId == schedule.Id &&
            s.Status == SessionStatus.Scheduled &&
            s.StartDateTime.Hour == 16 &&
            s.StartDateTime.Minute == 30);
    }

    [Fact]
    public async Task AddRangeAsync_IsCalledWithCorrectCount()
    {
        var handler = new GenerateSessionOccurrencesCommandHandler(
            _traineeGroupRepoMock.Object,
            _sessionOccurrenceRepoMock.Object,
            _mapperMock.Object);
        var scheduleMon = CreateSchedule(DayOfWeek.Monday, id: 1);
        var scheduleWed = CreateSchedule(DayOfWeek.Wednesday, id: 2);
        var group = CreateTraineeGroup(scheduleMon, scheduleWed);
        var command = CreateCommand(traineeGroupId: 1, durationInDays: 14);
        IEnumerable<SessionOccurrence>? capturedSessions = null;

        _traineeGroupRepoMock.Setup(r => r.GetByIdWithSchedulesAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(group);
        _sessionOccurrenceRepoMock.Setup(r => r.GetLastOccurrenceDateAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync((DateTime?)null);
        _sessionOccurrenceRepoMock
            .Setup(r => r.AddRangeAsync(It.IsAny<IEnumerable<SessionOccurrence>>(), It.IsAny<CancellationToken>()))
            .Callback<IEnumerable<SessionOccurrence>, CancellationToken>((sessions, _) => capturedSessions = sessions)
            .Returns(Task.CompletedTask);

        await handler.Handle(command, CancellationToken.None);

        capturedSessions.Should().NotBeNull();
        var sessionsList = capturedSessions!.ToList();
        sessionsList.Should().OnlyContain(s => s.GroupScheduleId == 1 || s.GroupScheduleId == 2);
        sessionsList.Should().OnlyContain(s => s.Status == SessionStatus.Scheduled);
    }

    [Fact]
    public async Task AddRangeAsync_IsNeverCalled_WhenNoSessionsToCreate()
    {
        var handler = new GenerateSessionOccurrencesCommandHandler(
            _traineeGroupRepoMock.Object,
            _sessionOccurrenceRepoMock.Object,
            _mapperMock.Object);
        var schedule = CreateSchedule(DayOfWeek.Monday);
        var group = CreateTraineeGroup(schedule);
        var command = CreateCommand(traineeGroupId: 1, durationInDays: 1);

        _traineeGroupRepoMock.Setup(r => r.GetByIdWithSchedulesAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(group);
        _sessionOccurrenceRepoMock.Setup(r => r.GetLastOccurrenceDateAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync((DateTime?)null);

        var result = await handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Data.Should().Be(0);
        _sessionOccurrenceRepoMock.Verify(
            r => r.AddRangeAsync(It.IsAny<IEnumerable<SessionOccurrence>>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }
}
