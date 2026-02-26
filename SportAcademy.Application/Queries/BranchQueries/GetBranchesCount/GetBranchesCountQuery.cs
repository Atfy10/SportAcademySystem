using MediatR;
using SportAcademy.Application.Common.Result;

namespace SportAcademy.Application.Queries.BranchQueries.GetBranchesCount;

public record GetBranchesCountQuery() : IRequest<Result<int>>; 

