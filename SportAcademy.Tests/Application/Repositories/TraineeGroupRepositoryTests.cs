using AutoMapper;
using FluentAssertions;
using Moq;
using SportAcademy.Application.Commands.SessionOccurrenceCommands.GenerateSessionOccurrences;
using SportAcademy.Application.Common.Pagination;
using SportAcademy.Application.DTOs.GroupScheduleDtos;
using SportAcademy.Application.DTOs.TraineeGroupDtos;
using SportAcademy.Application.Queries.TraineeGroupQueries.Search;
using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Entities;
using SportAcademy.Domain.Enums;
using SportAcademy.Domain.ValueObjects;
using SportAcademy.Domain.Exceptions.BaseExceptions;
using SportAcademy.Domain.Exceptions.SessionOccurrenceExceptions;
using SportAcademy.Domain.Exceptions.SharedExceptions;
using PagedDataListTraineeGroupDto = SportAcademy.Application.Common.Pagination.PagedData<SportAcademy.Application.DTOs.TraineeGroupDtos.ListTraineeGroupDto>;

namespace SportAcademy.Tests.Application.Repositories;

public class TraineeGroupRepositoryTests
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
    public async Task GetByIdWithSchedulesAsync_IsCalled_WithCorrectId()
    {
        var handler = new GenerateSessionOccurrencesCommandHandler(
            _traineeGroupRepoMock.Object,
            _sessionOccurrenceRepoMock.Object,
            _mapperMock.Object);
        var command = CreateCommand(traineeGroupId: 5);
        var group = CreateTraineeGroup(CreateSchedule());

        _traineeGroupRepoMock.Setup(r => r.GetByIdWithSchedulesAsync(5, It.IsAny<CancellationToken>()))
            .ReturnsAsync(group);
        _sessionOccurrenceRepoMock.Setup(r => r.GetLastOccurrenceDateAsync(5, It.IsAny<CancellationToken>()))
            .ReturnsAsync((DateTime?)null);
        _sessionOccurrenceRepoMock.Setup(r => r.AddRangeAsync(It.IsAny<IEnumerable<SessionOccurrence>>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        await handler.Handle(command, CancellationToken.None);

        _traineeGroupRepoMock.Verify(
            r => r.GetByIdWithSchedulesAsync(5, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task GetByIdWithSchedulesAsync_IsNeverCalled_WhenIdIsInvalid()
    {
        var handler = new GenerateSessionOccurrencesCommandHandler(
            _traineeGroupRepoMock.Object,
            _sessionOccurrenceRepoMock.Object,
            _mapperMock.Object);
        var command = CreateCommand(traineeGroupId: 0);

        var act = () => handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<IdNotFoundException>();
        _traineeGroupRepoMock.Verify(
            r => r.GetByIdWithSchedulesAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task GetByIdWithSchedulesAsync_IsCalled_WhenGroupNotFound()
    {
        var handler = new GenerateSessionOccurrencesCommandHandler(
            _traineeGroupRepoMock.Object,
            _sessionOccurrenceRepoMock.Object,
            _mapperMock.Object);
        var command = CreateCommand(traineeGroupId: 999);

        _traineeGroupRepoMock.Setup(r => r.GetByIdWithSchedulesAsync(999, It.IsAny<CancellationToken>()))
            .ReturnsAsync((TraineeGroup?)null);

        var act = () => handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<IdNotFoundException>();
        _traineeGroupRepoMock.Verify(
            r => r.GetByIdWithSchedulesAsync(999, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task GetByIdWithSchedulesAsync_IsCalled_WhenNoSchedulesFound()
    {
        var handler = new GenerateSessionOccurrencesCommandHandler(
            _traineeGroupRepoMock.Object,
            _sessionOccurrenceRepoMock.Object,
            _mapperMock.Object);
        var command = CreateCommand(traineeGroupId: 1);
        var group = CreateTraineeGroup();

        _traineeGroupRepoMock.Setup(r => r.GetByIdWithSchedulesAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(group);

        var act = () => handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<NoSchedulesFoundException>();
        _traineeGroupRepoMock.Verify(
            r => r.GetByIdWithSchedulesAsync(1, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task SearchAsync_IsCalledWithTrimmedTerm()
    {
        var handler = new SearchTraineeGroupsQueryHandler(
            _traineeGroupRepoMock.Object,
            _mapperMock.Object);
        var query = new SearchTraineeGroupsQuery("  swim  ", PageRequest.Create(1, 10));

        _traineeGroupRepoMock.Setup(r => r.SearchAsync("swim", query.Page, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PagedDataListTraineeGroupDto
            {
                Items = new List<ListTraineeGroupDto>(),
                TotalCount = 0, Page = 1, PageSize = 10
            });

        await handler.Handle(query, CancellationToken.None);

        _traineeGroupRepoMock.Verify(
            r => r.SearchAsync("swim", query.Page, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task SearchAsync_IsCalledWithCorrectPage()
    {
        var handler = new SearchTraineeGroupsQueryHandler(
            _traineeGroupRepoMock.Object,
            _mapperMock.Object);
        var pageRequest = PageRequest.Create(3, 50);
        var query = new SearchTraineeGroupsQuery("swimming", pageRequest);

        _traineeGroupRepoMock.Setup(r => r.SearchAsync("swimming", pageRequest, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PagedDataListTraineeGroupDto
            {
                Items = new List<ListTraineeGroupDto>(),
                TotalCount = 0, Page = 3, PageSize = 50
            });

        await handler.Handle(query, CancellationToken.None);

        _traineeGroupRepoMock.Verify(
            r => r.SearchAsync("swimming", pageRequest, It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
