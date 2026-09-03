using MediatR;
using SportAcademy.Application.Common.Pagination;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.DTOs.ExcuseRequestDtos;

namespace SportAcademy.Application.Queries.ExcuseRequestQueries.GetPending;

public record GetPendingExcuseRequestsQuery(PageRequest Page) : IRequest<Result<PagedData<ExcuseRequestDto>>>, IPaginatedRequest
{
    public PageRequest Page { get; set; } = Page;
}
