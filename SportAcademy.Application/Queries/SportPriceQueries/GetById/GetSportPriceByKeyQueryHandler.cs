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
using SportAcademy.Domain.Entities;
using SportAcademy.Domain.Enums;
using SportAcademy.Domain.Exceptions;

namespace SportAcademy.Application.Queries.SportPriceQueries.GetById
{
	public class GetSportPriceByKeyQueryHandler : IRequestHandler<GetSportPriceByKeyQuery, Result<SportPriceDto>>
	{
		private readonly ISportPriceRepository _sportPriceRepository;
		private readonly IMapper _mapper;
		private readonly string _operationType = OperationType.Get.ToString();

		public GetSportPriceByKeyQueryHandler(ISportPriceRepository sportPriceRepository, IMapper mapper)
		{
			_sportPriceRepository = sportPriceRepository;
			_mapper = mapper;
		}

		public async Task<Result<SportPriceDto>> Handle(GetSportPriceByKeyQuery request, CancellationToken cancellationToken)
		{
			var sportPrice = await _sportPriceRepository.GetByKeyWithIncludesAsync(request.BranchId,
				request.SportId, request.SubsTypeId, cancellationToken);

			if (sportPrice is null)
				throw new SportPriceNotFoundException();

			var dto = _mapper.Map<SportPriceDto>(sportPrice);
			return Result<SportPriceDto>.Success(dto, _operationType);
		}
	}

}
