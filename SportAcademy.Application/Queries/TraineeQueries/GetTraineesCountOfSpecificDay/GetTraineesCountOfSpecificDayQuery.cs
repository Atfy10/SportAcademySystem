using MediatR;
using SportAcademy.Application.Common.Result;

namespace SportAcademy.Application.Queries.TraineeQueries.GetTraineesCountOfSpecificDay
{
    public record GetTraineesCountOfSpecificDayQuery(DateTime Date) : IRequest<Result<int>>;
}
