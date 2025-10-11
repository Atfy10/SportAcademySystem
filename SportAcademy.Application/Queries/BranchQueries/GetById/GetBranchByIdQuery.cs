using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using SportAcademy.Application.DTOs.BranchDtos;
using SportAcademy.Application.Services;

namespace SportAcademy.Application.Queries.BranchQueries.GetById
{
	public record GetBranchByIdQuery(int Id) : IRequest<Result<BranchDto>>;

}
