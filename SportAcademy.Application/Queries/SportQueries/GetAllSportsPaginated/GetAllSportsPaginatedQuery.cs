using MediatR;
using SportAcademy.Application.Common.Pagination;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.DTOs.SportDtos;

namespace SportAcademy.Application.Queries.SportQueries.GetAll
{
    public record GetAllSportsPaginatedQuery(PageRequest Page)
        : IRequest<Result<PagedData<SportDto>>>, IPaginatedRequest
    {
        public PageRequest Page { get; set; } = Page;
    }
}
