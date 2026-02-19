using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.DTOs.SportPriceDtos;
using SportAcademy.Domain.Entities;

namespace SportAcademy.Application.Queries.SportPriceQueries.GetAll
{
	public record GetAllSportPricesQuery() : IRequest<Result<List<SportPriceDto>>>;

}
