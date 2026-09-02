using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.DTOs.TraineeDtos;

namespace SportAcademy.Application.Queries.TraineeQueries.GetCoachHistory
{
    public record GetTraineeCoachHistoryQuery(int TraineeId) : IRequest<Result<List<CoachHistoryEntryDto>>>;
}
