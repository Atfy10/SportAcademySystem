using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Enums;
using SportAcademy.Domain.Exceptions.SportExceptions;

namespace SportAcademy.Application.Commands.SportPriceCommands.DeleteSportPrice
{
	public class DeleteSportPriceCommandHandler : IRequestHandler<DeleteSportPriceCommand, Result<bool>>
	{
		private readonly ISportPriceRepository _sportPriceRepository;
		private readonly string _operationType = OperationType.Delete.ToString();

		public DeleteSportPriceCommandHandler(ISportPriceRepository sportPriceRepository)
		{
			_sportPriceRepository = sportPriceRepository;
		}

		public async Task<Result<bool>> Handle(DeleteSportPriceCommand request, CancellationToken cancellationToken)
		{
			var keyExists = await _sportPriceRepository.IsExistAsync(request.BranchId,
				request.SportId, request.SubsTypeId, request.GroupType, cancellationToken);
			if (!keyExists)
				throw new SportPriceNotFoundException($"{request.BranchId}, {request.SportId}, {request.SubsTypeId}, {request.GroupType}");

			cancellationToken.ThrowIfCancellationRequested();

			var sportPrice = await _sportPriceRepository
				.GetByKeyAsync(request.BranchId, request.SportId, request.SubsTypeId, request.GroupType, cancellationToken)
				?? throw new SportPriceNotFoundException($"{request.BranchId}, {request.SportId}, {request.SubsTypeId}, {request.GroupType}");

			await _sportPriceRepository.DeleteAsync(sportPrice, cancellationToken);

			return Result<bool>.Success(true, _operationType);
		}
	}
}
