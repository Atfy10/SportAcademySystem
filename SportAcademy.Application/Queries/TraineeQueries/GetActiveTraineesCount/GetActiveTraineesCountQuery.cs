using MediatR;
using SportAcademy.Application.Common.Result;

namespace SportAcademy.Application.Queries.TraineeQueries.GetActiveTraineesCount;

public record GetActiveTraineesCountQuery() : IRequest<Result<int>>;