using MediatR;
using SportAcademy.Application.Common.Pagination;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.DTOs.TraineeDtos;

namespace SportAcademy.Application.Queries.TraineeQueries.GetAll
{
    public record GetAllTraineesQuery(
        PageRequest Page,
        string? Sport = null,
        string? Status = null,
        string? SortBy = null,
        string? SortDir = null
    ) : IRequest<Result<PagedData<TraineeCardDto>>>, IPaginatedRequest
    {
        public PageRequest Page { get; set; } = Page;
    }
}
