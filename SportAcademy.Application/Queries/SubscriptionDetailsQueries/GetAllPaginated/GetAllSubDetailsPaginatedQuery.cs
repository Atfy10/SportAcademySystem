using MediatR;
using SportAcademy.Application.Common.Pagination;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.DTOs.SubscriptionDetailsDtos;

namespace SportAcademy.Application.Queries.SubscriptionDetailsQueries.GetAllPaginated
{
    public record GetAllSubDetailsPaginatedQuery(
        PageRequest Page,
        string? Term = null
    ) : IRequest<Result<PagedData<SubscriptionDetailsDto>>>, IPaginatedRequest
    {
        public PageRequest Page { get; set; } = Page;
    }
}
