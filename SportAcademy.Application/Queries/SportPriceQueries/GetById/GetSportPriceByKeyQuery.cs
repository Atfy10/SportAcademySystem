using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using SportAcademy.Application.DTOs.SportPriceDtos;
using SportAcademy.Application.Services;
using SportAcademy.Domain.Entities;

namespace SportAcademy.Application.Queries.SportPriceQueries.GetById
{
	public record GetSportPriceByKeyQuery(
		int BranchId,
		int SportId,
		int SubsTypeId
	) : IRequest<Result<SportPriceDto>>;

}
