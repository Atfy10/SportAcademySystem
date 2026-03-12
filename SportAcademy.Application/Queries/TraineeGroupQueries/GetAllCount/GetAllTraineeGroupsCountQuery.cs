using MediatR;
using SportAcademy.Application.Common.Result;

namespace SportAcademy.Application.Queries.TraineeGroupQueries.GetAllCount;

public record GetAllTraineeGroupsCountQuery() : IRequest<Result<int>>;