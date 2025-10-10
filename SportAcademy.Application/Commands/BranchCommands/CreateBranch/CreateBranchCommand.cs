using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using SportAcademy.Application.Services;

namespace SportAcademy.Application.Commands.BranchCommands.CreateBranch
{
	public record CreateBranchCommand(
		string Name,
		string City,
		string Country,
		string PhoneNumber,
		string? Email,
		string CoX,
		string CoY
		) : IRequest<Result<int>>;

}
