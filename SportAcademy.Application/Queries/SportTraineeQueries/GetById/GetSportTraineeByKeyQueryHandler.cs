using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.DTOs.SportTraineeDtos;
using SportAcademy.Application.Interfaces;
using SportAcademy.Application.Mappings;
using SportAcademy.Domain.Enums;
using SportAcademy.Domain.Exceptions.SportExceptions;

namespace SportAcademy.Application.Queries.SportTraineeQueries.GetById
{
	public class GetSportTraineeByKeyQueryHandler : IRequestHandler<GetSportTraineeByKeyQuery, Result<SportTraineeDto>>
	{	
		private readonly ISportTraineeRepository _sportTraineeRepository;
		private readonly string _operationType = OperationType.Get.ToString();

		public GetSportTraineeByKeyQueryHandler(ISportTraineeRepository sportTraineeRepository)
		{
			_sportTraineeRepository = sportTraineeRepository;
		}

		public async Task<Result<SportTraineeDto>> Handle(GetSportTraineeByKeyQuery request, CancellationToken cancellationToken)
		{
			var entity = await _sportTraineeRepository
				.GetByIdWithIncludesAsync(request.SportId, request.TraineeId, cancellationToken)
				?? throw new SportTraineeNotFoundException($"{request.SportId}, {request.TraineeId}");

            var dto = entity.ToDto();

            return Result<SportTraineeDto>.Success(dto, _operationType);
		}
	}
}
