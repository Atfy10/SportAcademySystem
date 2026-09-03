using MediatR;
using SportAcademy.Application.Common.Result;

namespace SportAcademy.Application.Queries.AuthQueries.GetUserBranches;

public record GetUserBranchesQuery(Guid UserId) : IRequest<Result<List<int>>>;
