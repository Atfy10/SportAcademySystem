using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.DTOs.BranchDtos;

namespace SportAcademy.Application.Commands.BranchCommands.UpdateBranch
{
	public record UpdateBranchCommand(
		int Id,
		string Name,
		string City,
		string Country,
		string PhoneNumber,
		string? Email,
		string CoX,
		string CoY,
		bool IsActive
		) : IRequest<Result<BranchDto>>;
}
