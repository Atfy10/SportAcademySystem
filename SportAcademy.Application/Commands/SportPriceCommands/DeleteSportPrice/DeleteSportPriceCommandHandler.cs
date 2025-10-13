using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using SportAcademy.Application.Interfaces;
using SportAcademy.Application.Services;
using SportAcademy.Domain.Enums;
using SportAcademy.Domain.Exceptions;

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
			var keyExists = await _sportPriceRepository
				.CheckIfKeyExists(request.BranchId, request.SportId, request.SubsTypeId, cancellationToken);

			if (!keyExists)
				throw new SportPriceNotFoundException();

			cancellationToken.ThrowIfCancellationRequested();

			var sportPrice = await _sportPriceRepository
				.GetByKeyAsync(request.BranchId, request.SportId, request.SubsTypeId, cancellationToken)
				?? throw new SportPriceNotFoundException();

			await _sportPriceRepository.DeleteAsync(sportPrice, cancellationToken);

			return Result<bool>.Success(true, _operationType);
		}
	}
}
