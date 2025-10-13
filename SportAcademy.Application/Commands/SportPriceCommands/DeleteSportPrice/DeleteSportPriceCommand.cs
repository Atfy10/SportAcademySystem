using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using SportAcademy.Application.Services;

namespace SportAcademy.Application.Commands.SportPriceCommands.DeleteSportPrice
{
	public record DeleteSportPriceCommand(
		int SportId,
		int BranchId,
		int SubsTypeId
	) : IRequest<Result<bool>>;

}
