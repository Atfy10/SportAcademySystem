using MediatR;
using SportAcademy.Application.Common.Pagination;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.Common.Search;
using SportAcademy.Application.DTOs.EmployeeDtos;

namespace SportAcademy.Application.Queries.EmployeeQueries.SearchEmployeess;

public record SearchEmployeeQuery(
    string SearchTerm,
    PageRequest Page
) : IRequest<Result<PagedData<EmployeeCardDto>>>, IPaginatedRequest, ISearchRequest
{
    public PageRequest Page { get; set; } = Page;

    public string Term { get; } = SearchTerm;
}
