using MediatR;
using SportAcademy.Application.Common.Result;

namespace SportAcademy.Application.Queries.EmployeeQueries.GetActiveCoachesCount;

public record GetActiveCoachesCountQuery() : IRequest<Result<int>>;
