using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.Interfaces;

namespace SportAcademy.Application.Commands.BranchCommands.DeleteBranch
{
	public record DeleteBranchCommand(int Id): IRequest<Result<bool>>, IRequiresFeature
	{
		public string FeatureKey => "branch-management";
	}
}
