using AutoMapper;
using FluentAssertions;
using Moq;
using SportAcademy.Application.Common.Pagination;
using SportAcademy.Application.DTOs.TraineeDtos;
using SportAcademy.Application.Interfaces;
using SportAcademy.Application.Queries.TraineeQueries.GetAll;

namespace SportAcademy.Tests.Application.Handlers;

public class GetAllTraineesQueryHandlerTests
{
    private readonly Mock<IMapper> _mapperMock = new();
    private readonly Mock<IAttendanceRepository> _attendanceRepoMock = new();
    private readonly Mock<ITraineeRepository> _traineeRepoMock = new();
    private readonly GetAllTraineesQueryHandler _handler;

    public GetAllTraineesQueryHandlerTests()
    {
        _handler = new GetAllTraineesQueryHandler(
            _attendanceRepoMock.Object,
            _traineeRepoMock.Object
        );
    }

    [Fact]
    public async Task Handle_ReturnsPagedResult()
    {
        var page = PageRequest.Create(1, 10);
        var query = new GetAllTraineesQuery(page);
        var pagedData = new PagedData<TraineeDto>
        {
            Items = [],
            TotalCount = 0,
            Page = 1,
            PageSize = 10
        };

        _traineeRepoMock
            .Setup(r => r.GetAllPaginatedAsync<TraineeDto>(page, It.IsAny<CancellationToken>()))
            .ReturnsAsync(pagedData);

        var result = await _handler.Handle(query, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.Items.Should().BeEmpty();
        result.Data.TotalCount.Should().Be(0);
    }

    [Fact]
    public async Task Handle_PassesPageRequestToRepository()
    {
        var page = PageRequest.Create(2, 25);
        var query = new GetAllTraineesQuery(page);
        var pagedData = new PagedData<TraineeDto>
        {
            Items = [],
            TotalCount = 50,
            Page = 2,
            PageSize = 25
        };

        _traineeRepoMock
            .Setup(r => r.GetAllPaginatedAsync<TraineeDto>(page, It.IsAny<CancellationToken>()))
            .ReturnsAsync(pagedData);

        var result = await _handler.Handle(query, CancellationToken.None);

        _traineeRepoMock.Verify(
            r => r.GetAllPaginatedAsync<TraineeDto>(page, It.IsAny<CancellationToken>()),
            Times.Once);
        result.Data!.Page.Should().Be(2);
        result.Data.PageSize.Should().Be(25);
    }
}
