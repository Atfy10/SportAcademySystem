using MediatR;
using SportAcademy.Application.Common.Pagination;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.DTOs.TraineeGroupDtos;

namespace SportAcademy.Application.Queries.TraineeGroupQueries.GetAllOfSpecificDay;

public record GetAllSessionsOfSpecificDayQuery(
    DateTime Day,
    PageRequest PageRequest
) : IRequest<Result<PagedData<ListTraineeGroupDto>>>, IPaginatedRequest
{
    public PageRequest Page { get; set; } = PageRequest;
}
