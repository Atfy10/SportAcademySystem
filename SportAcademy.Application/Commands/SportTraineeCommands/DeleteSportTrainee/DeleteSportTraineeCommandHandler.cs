using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using SportAcademy.Application.Interfaces;
using SportAcademy.Application.Services;
using SportAcademy.Domain.Entities;
using SportAcademy.Domain.Enums;
using SportAcademy.Domain.Exceptions;

namespace SportAcademy.Application.Commands.SportTraineeCommands.DeleteSportTrainee
{
	public class DeleteSportTraineeCommandHandler : IRequestHandler<DeleteSportTraineeCommand, Result<string>>
	{
		private readonly ISportTraineeRepository _sportTraineeRepository;
		private readonly string _operationType = OperationType.Delete.ToString();

		public DeleteSportTraineeCommandHandler(ISportTraineeRepository sportTraineeRepository)
		{
			_sportTraineeRepository = sportTraineeRepository;
		}

		public async Task<Result<string>> Handle(DeleteSportTraineeCommand request, CancellationToken cancellationToken)
		{
			var exists = await _sportTraineeRepository.CheckIfKeyExists(request.SportId, request.TraineeId, cancellationToken);
			if (!exists)
				throw new SportTraineeNotFoundException();

			var entity = _sportTraineeRepository.GetByIdAsync(request.SportId, request.TraineeId, cancellationToken).Result
				?? throw new SportTraineeNotFoundException();

			await _sportTraineeRepository.DeleteAsync(entity, cancellationToken);

			return Result<string>.Success("SportTrainee deleted successfully", _operationType);
		}
	}
}
