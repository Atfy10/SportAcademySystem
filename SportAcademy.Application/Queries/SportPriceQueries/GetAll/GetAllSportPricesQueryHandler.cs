using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using SportAcademy.Application.DTOs.SportPriceDtos;
using SportAcademy.Application.Interfaces;
using SportAcademy.Application.Services;
using SportAcademy.Domain.Enums;

namespace SportAcademy.Application.Queries.SportPriceQueries.GetAll
{
	public class GetAllSportPricesQueryHandler : IRequestHandler<GetAllSportPricesQuery, Result<List<SportPriceDto>>>
	{
		private readonly ISportPriceRepository _sportPriceRepository;
		private readonly IMapper _mapper;
		private readonly string _operationType = OperationType.GetAll.ToString();

		public GetAllSportPricesQueryHandler(ISportPriceRepository sportPriceRepository, IMapper mapper)
		{
			_sportPriceRepository = sportPriceRepository;
			_mapper = mapper;
		}

		public async Task<Result<List<SportPriceDto>>> Handle(GetAllSportPricesQuery request, CancellationToken cancellationToken)
		{
			var prices = await _sportPriceRepository.GetAllWithIncludesAsync(cancellationToken) 
				?? [];

			var dtoList = _mapper.Map<List<SportPriceDto>>(prices);

			return Result<List<SportPriceDto>>.Success(dtoList, _operationType);
		}
	}
}
