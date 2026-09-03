using MediatR;
using SportAcademy.Application.Common.Result;

namespace SportAcademy.Application.Queries.CoachQueries.GetCoachBranches;

public record GetCoachBranchesQuery(int CoachId) : IRequest<Result<List<int>>>;
