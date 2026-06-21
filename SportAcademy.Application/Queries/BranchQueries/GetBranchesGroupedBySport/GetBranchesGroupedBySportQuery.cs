using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.DTOs.BranchDtos;

namespace SportAcademy.Application.Queries.BranchQueries.GetBranchesGroupedBySport
{
    public record GetBranchesGroupedBySportQuery() : IRequest<Result<List<SportBranchGroupDto>>>;
}
