using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.DTOs.BranchDtos;

namespace SportAcademy.Application.Queries.BranchQueries.GetAll
{
	public record GetAllBranchesQuery () : IRequest<Result<List<BranchDropDownListDto>>>;
}
