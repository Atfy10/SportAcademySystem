using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.DTOs.SportTraineeDtos;
using SportAcademy.Application.Interfaces;
using SportAcademy.Application.Mappings;
using SportAcademy.Domain.Entities;
using SportAcademy.Domain.Enums;
using SportAcademy.Domain.Exceptions.SharedExceptions;
using SportAcademy.Domain.Exceptions.SportExceptions;

namespace SportAcademy.Application.Commands.SportTraineeCommands.UpdateSportTrainee
{
	public class UpdateSportTraineeCommandHandler : IRequestHandler<UpdateSportTraineeCommand, Result<SportTraineeDto>>
	{
		private readonly ISportTraineeRepository _sportTraineeRepository;
		private readonly string _operationType = OperationType.Update.ToString();

		public UpdateSportTraineeCommandHandler(
			ISportTraineeRepository sportTraineeRepository)
		{
			_sportTraineeRepository = sportTraineeRepository;
		}

		public async Task<Result<SportTraineeDto>> Handle(UpdateSportTraineeCommand request, CancellationToken cancellationToken)
		{
			var exists = await _sportTraineeRepository.IsExistAsync(request.SportId, request.TraineeId, cancellationToken);
			if (!exists)
				throw new SportTraineeNotFoundException($"{request.SportId}, {request.TraineeId}");

            if (!Enum.IsDefined(typeof(SkillLevel), request.SkillLevel))
				throw new InvalidSkillLevelException();

			cancellationToken.ThrowIfCancellationRequested();

			var sportTrainee = SportTrainee.Create(request.SportId, request.TraineeId, Enum.Parse<SkillLevel>(request.SkillLevel));
			var dto = sportTrainee.ToDto();

            await _sportTraineeRepository.UpdateAsync(sportTrainee, cancellationToken);

			return Result<SportTraineeDto>.Success(dto, _operationType);
		}
	}
}
