using AutoMapper;
using FluentAssertions;
using Moq;
using SportAcademy.Application.Common.Pagination;
using SportAcademy.Application.DTOs.GroupScheduleDtos;
using SportAcademy.Application.DTOs.TraineeGroupDtos;
using SportAcademy.Application.Queries.TraineeGroupQueries.Search;
using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Exceptions.SharedExceptions;

namespace SportAcademy.Tests.Application.Handlers;

public class SearchTraineeGroupsQueryHandlerTests
{
    private readonly Mock<ITraineeGroupRepository> _traineeGroupRepoMock = new();
    private readonly Mock<IMapper> _mapperMock = new();
    private readonly SearchTraineeGroupsQueryHandler _handler;

    public SearchTraineeGroupsQueryHandlerTests()
    {
        _handler = new SearchTraineeGroupsQueryHandler(
            _traineeGroupRepoMock.Object,
            _mapperMock.Object);
    }

    private static SearchTraineeGroupsQuery CreateQuery(string searchTerm = "swim", int page = 1, int pageSize = 10)
    {
        return new SearchTraineeGroupsQuery(searchTerm, PageRequest.Create(page, pageSize));
    }

    private static PagedData<ListTraineeGroupDto> CreatePagedData()
    {
        return new PagedData<ListTraineeGroupDto>
        {
            Items = new List<ListTraineeGroupDto>
            {
                new(1, "Swimming", "Ahmed Ali", "Main Branch", 55, 10, new List<GroupScheduleItemDto>())
            },
            TotalCount = 1,
            Page = 1,
            PageSize = 10
        };
    }

    [Fact]
    public async Task Handle_ValidSearchTerm_ReturnsSuccessResult()
    {
        var query = CreateQuery("swim");
        var pagedData = CreatePagedData();

        _traineeGroupRepoMock.Setup(r => r.SearchAsync("swim", query.Page, It.IsAny<CancellationToken>()))
            .ReturnsAsync(pagedData);

        var result = await _handler.Handle(query, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.Items.Should().HaveCount(1);
        result.Data.TotalCount.Should().Be(1);
    }

    [Fact]
    public async Task Handle_ValidSearchTerm_TrimsTerm()
    {
        var query = CreateQuery("  swim  ");
        var pagedData = CreatePagedData();

        _traineeGroupRepoMock.Setup(r => r.SearchAsync("swim", query.Page, It.IsAny<CancellationToken>()))
            .ReturnsAsync(pagedData);

        await _handler.Handle(query, CancellationToken.None);

        _traineeGroupRepoMock.Verify(
            r => r.SearchAsync("swim", query.Page, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ValidSearchTerm_CallsRepository()
    {
        var query = CreateQuery("swimming", 2, 25);
        var pagedData = new PagedData<ListTraineeGroupDto>
        {
            Items = new List<ListTraineeGroupDto>(),
            TotalCount = 0,
            Page = 2,
            PageSize = 25
        };

        _traineeGroupRepoMock.Setup(r => r.SearchAsync("swimming", query.Page, It.IsAny<CancellationToken>()))
            .ReturnsAsync(pagedData);

        await _handler.Handle(query, CancellationToken.None);

        _traineeGroupRepoMock.Verify(
            r => r.SearchAsync("swimming", query.Page, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_SearchTermLessThan2Chars_ThrowsInvalidSearchTermException()
    {
        var query = CreateQuery("a");

        var act = () => _handler.Handle(query, CancellationToken.None);

        await act.Should().ThrowAsync<InvalidSearchTermException>()
            .Where(e => e.MinLength == 2);
    }

    [Fact]
    public async Task Handle_NullSearchTerm_ThrowsInvalidSearchTermException()
    {
        var query = new SearchTraineeGroupsQuery(null!, PageRequest.Create(1, 10));

        var act = () => _handler.Handle(query, CancellationToken.None);

        await act.Should().ThrowAsync<InvalidSearchTermException>()
            .Where(e => e.MinLength == 2);
    }

    [Fact]
    public async Task Handle_EmptySearchTerm_ThrowsInvalidSearchTermException()
    {
        var query = CreateQuery("");

        var act = () => _handler.Handle(query, CancellationToken.None);

        await act.Should().ThrowAsync<InvalidSearchTermException>()
            .Where(e => e.MinLength == 2);
    }

    [Fact]
    public async Task Handle_WhitespaceOnlySearchTerm_ThrowsInvalidSearchTermException()
    {
        var query = CreateQuery("   ");

        var act = () => _handler.Handle(query, CancellationToken.None);

        await act.Should().ThrowAsync<InvalidSearchTermException>()
            .Where(e => e.MinLength == 2);
    }
}
