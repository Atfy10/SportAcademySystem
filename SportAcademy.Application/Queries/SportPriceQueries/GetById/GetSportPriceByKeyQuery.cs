using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.DTOs.SportPriceDtos;
using SportAcademy.Domain.Entities;
using SportAcademy.Domain.Enums;

namespace SportAcademy.Application.Queries.SportPriceQueries.GetById
{
	public record GetSportPriceByKeyQuery(
		int BranchId,
		int SportId,
		int SubsTypeId,
		TraineeGroupType GroupType
	) : IRequest<Result<SportPriceDto>>;

}
