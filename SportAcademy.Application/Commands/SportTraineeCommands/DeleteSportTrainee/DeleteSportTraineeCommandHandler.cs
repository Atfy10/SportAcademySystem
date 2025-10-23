using MediatR;
using SportAcademy.Application.Interfaces;
using SportAcademy.Application.Services;
using SportAcademy.Domain.Enums;
using SportAcademy.Domain.Exceptions.SportExceptions;

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
			var entity = await _sportTraineeRepository
				.GetByIdsAsync(cancellationToken, request.SportId, request.TraineeId)
				?? throw new SportTraineeNotFoundException($"{request.SportId}, {request.TraineeId}");

			await _sportTraineeRepository.DeleteAsync(entity, cancellationToken);

			return Result<string>.Success("SportTrainee deleted successfully", _operationType);
		}
	}
}
