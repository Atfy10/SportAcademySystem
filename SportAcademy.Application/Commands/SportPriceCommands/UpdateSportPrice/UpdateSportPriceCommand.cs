using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Enums;

namespace SportAcademy.Application.Commands.SportPriceCommands.UpdateSportPrice
{
	public record UpdateSportPriceCommand(
		int SportId,
		int BranchId,
		int SubsTypeId,
		TraineeGroupType GroupType,
		decimal NewPrice
	) : IRequest<Result<decimal>>, IBranchScopedRequest, IRequiresFeature
	{
		public string FeatureKey => "pricing-management";
	}

}
