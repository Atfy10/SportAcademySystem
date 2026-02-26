using MediatR;
using SportAcademy.Application.Common.Pagination;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.DTOs.CoachDtos;

namespace SportAcademy.Application.Queries.EmployeeQueries.GetAllCoachs;

public record GetAllCoachsQuery(
    PageRequest Page
) : IRequest<Result<PagedData<CoachCardDto>>>, IPaginatedRequest
{
    public PageRequest Page { get; set; } = Page;
}

