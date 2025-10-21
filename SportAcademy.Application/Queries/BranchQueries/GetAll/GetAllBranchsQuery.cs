using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using SportAcademy.Application.DTOs.BranchDtos;
using SportAcademy.Application.Services;

namespace SportAcademy.Application.Queries.BranchQueries.GetAll
{
	public record GetAllAttendancesQuery () : IRequest<Result<List<BranchDto>>>;

}
