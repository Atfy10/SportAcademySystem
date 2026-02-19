using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.DTOs.TraineeGroupDtos;

namespace SportAcademy.Application.Queries.TraineeGroupQueries.GetAllOfSpecificDay;

public record GetAllSessionsOfSpecificDayQuery(
    DateTime Day
) : IRequest<Result<List<ListTraineeGroupDto>>>;
