using MediatR;
using SportAcademy.Application.Common.Pagination;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.Common.Search;
using SportAcademy.Application.DTOs.CoachDtos;

namespace SportAcademy.Application.Queries.CoachQueries.SearchCoachs;

public record SearchCoachQuery(
    string SearchTerm,
    PageRequest Page
) : IRequest<Result<PagedData<CoachCardDto>>>, IPaginatedRequest, ISearchRequest
{
    public PageRequest Page { get; set; } = Page;

    public string Term { get; } = SearchTerm;
}