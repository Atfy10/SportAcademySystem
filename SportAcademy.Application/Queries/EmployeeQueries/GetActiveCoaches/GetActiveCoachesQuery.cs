using MediatR;
using SportAcademy.Application.Common.Pagination;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.DTOs.EmployeeDtos;

namespace SportAcademy.Application.Queries.EmployeeQueries.GetActiveCoaches;

public record GetActiveCoachesQuery(PageRequest Page)
    : IRequest<Result<PagedData<EmployeeDto>>>, IPaginatedRequest
{
    public PageRequest Page { get; set; } = Page;
}