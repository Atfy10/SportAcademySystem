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

namespace SportAcademy.Application.Queries.SportTraineeQueries.GetAll
{
	public class GetAllSportTraineesQueryHandler : IRequestHandler<GetAllSportTraineesQuery, Result<List<SportTraineeDto>>>
	{
		private readonly ISportTraineeRepository _sportTraineeRepository;
		private readonly IMapper _mapper;
		private readonly string _operationType = OperationType.GetAll.ToString();


		public GetAllSportTraineesQueryHandler(ISportTraineeRepository sportTraineeRepository, IMapper mapper)
		{
			_sportTraineeRepository = sportTraineeRepository;
			_mapper = mapper;
		}

		public async Task<Result<List<SportTraineeDto>>> Handle(GetAllSportTraineesQuery request, CancellationToken cancellationToken)
		{
			var entities = await _sportTraineeRepository.GetAllAsyncWithIncludeAsync(cancellationToken)
				?? [];

			var dtos = _mapper.Map<List<SportTraineeDto>>(entities)
				?? throw new AutoMapperMappingException("Error occurred while mapping.");

            return Result<List<SportTraineeDto>>.Success(dtos, _operationType);
		}
	}
}
