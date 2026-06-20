using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.DTOs.SportTraineeDtos;
using SportAcademy.Application.Interfaces;
using SportAcademy.Application.Mappings;
using SportAcademy.Domain.Enums;

namespace SportAcademy.Application.Queries.SportTraineeQueries.GetAll
{
	public class GetAllSportTraineesQueryHandler : IRequestHandler<GetAllSportTraineesQuery, Result<List<SportTraineeDto>>>
	{
		private readonly ISportTraineeRepository _sportTraineeRepository;
		private readonly string _operationType = OperationType.GetAll.ToString();

		public GetAllSportTraineesQueryHandler(ISportTraineeRepository sportTraineeRepository)
		{
			_sportTraineeRepository = sportTraineeRepository;
		}

		public async Task<Result<List<SportTraineeDto>>> Handle(GetAllSportTraineesQuery request, CancellationToken cancellationToken)
		{
			var entities = await _sportTraineeRepository.GetAllAsyncWithIncludeAsync(cancellationToken)
				?? [];

			var dtos = entities.Select(e => e.ToDto()).ToList();

            return Result<List<SportTraineeDto>>.Success(dtos, _operationType);
		}
	}
}
