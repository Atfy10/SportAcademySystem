using MediatR;
using SportAcademy.Application.Common.Pagination;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.DTOs.SportDtos;

namespace SportAcademy.Application.Queries.SportQueries.SearchSports;

public record SearchSportsQuery(string SearchTerm, PageRequest Page)
        : IRequest<Result<PagedData<SportDto>>>, IPaginatedRequest
{
    public PageRequest Page { get; set; } = Page;
}
