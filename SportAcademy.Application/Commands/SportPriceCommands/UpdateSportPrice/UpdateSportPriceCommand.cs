using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using SportAcademy.Application.Common.Result;

namespace SportAcademy.Application.Commands.SportPriceCommands.UpdateSportPrice
{
	public record UpdateSportPriceCommand(
		int SportId,
		int BranchId,
		int SubsTypeId,
		decimal NewPrice
	) : IRequest<Result<decimal>>;

}
