using MediatR;
using SportAcademy.Application.DTOs.SportPriceDtos;
using SportAcademy.Application.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SportAcademy.Application.Commands.SportPriceCommands.CreateSportPrice
{
	public record CreateSportPriceCommand(
		int SportId,
		int BranchId,
		int SubsTypeId,
		decimal Price
	) : IRequest<Result<SportPriceBranchDto>>;

}
