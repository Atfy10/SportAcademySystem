using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using SportAcademy.Application.DTOs.SportTraineeDtos;
using SportAcademy.Application.Interfaces;
using SportAcademy.Application.Services;
using SportAcademy.Domain.Enums;
using SportAcademy.Domain.Exceptions;

namespace SportAcademy.Application.Queries.SportTraineeQueries.GetById
{
	public class GetSportTraineeByKeyQueryHandler : IRequestHandler<GetSportTraineeByKeyQuery, Result<SportTraineeDto>>
	{
		private readonly ISportTraineeRepository _sportTraineeRepository;
		private readonly IMapper _mapper;
		private readonly string _operationType = OperationType.Get.ToString();


		public GetSportTraineeByKeyQueryHandler(ISportTraineeRepository sportTraineeRepository, IMapper mapper)
		{
			_sportTraineeRepository = sportTraineeRepository;
			_mapper = mapper;
		}

		public async Task<Result<SportTraineeDto>> Handle(GetSportTraineeByKeyQuery request, CancellationToken cancellationToken)
		{
			var entity = await _sportTraineeRepository.GetByIdWithIncludesAsync(request.SportId,
				request.TraineeId, cancellationToken);
			if (entity is null)
				throw new SportTraineeNotFoundException();

			var dto = _mapper.Map<SportTraineeDto>(entity);
			return Result<SportTraineeDto>.Success(dto, _operationType);
		}
	}
}
