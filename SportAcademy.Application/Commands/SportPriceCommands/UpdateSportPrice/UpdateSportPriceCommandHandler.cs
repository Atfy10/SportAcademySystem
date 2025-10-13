using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using SportAcademy.Application.Interfaces;
using SportAcademy.Application.Services;
using SportAcademy.Domain.Enums;
using SportAcademy.Domain.Exceptions;

namespace SportAcademy.Application.Commands.SportPriceCommands.UpdateSportPrice
{
	public class UpdateSportPriceCommandHandler : IRequestHandler<UpdateSportPriceCommand, Result<decimal>>
	{
		private readonly ISportPriceRepository _sportPriceRepository;
		private readonly ISportRepository _sportRepository;
		private readonly IBranchRepository _branchRepository;
		private readonly ISubscriptionTypeRepository _subscriptionTypeRepository;
		private readonly string _operationType = OperationType.Update.ToString();
		private readonly IMapper _mapper;

		public UpdateSportPriceCommandHandler(
			ISportPriceRepository sportPriceRepository,
			ISportRepository sportRepository,
			IBranchRepository branchRepository,
			ISubscriptionTypeRepository subscriptionTypeRepository,
			IMapper mapper)
		{
			_sportPriceRepository = sportPriceRepository;
			_sportRepository = sportRepository;
			_branchRepository = branchRepository;
			_subscriptionTypeRepository = subscriptionTypeRepository;
			_mapper = mapper;
		}

		public async Task<Result<decimal>> Handle(UpdateSportPriceCommand request, CancellationToken cancellationToken)
		{
			var keyExists = await _sportPriceRepository
				.CheckIfKeyExists(request.BranchId, request.SportId, request.SubsTypeId, cancellationToken);
			if (!keyExists)
				throw new SportPriceNotFoundException();

			var branchExists = await _branchRepository.CheckIfBranchExists(request.BranchId, cancellationToken);
			if (!branchExists)
				throw new BranchNotFoundException();

			var sportExists = await _sportRepository.CheckIfSportExists(request.SportId, cancellationToken);
			if (!sportExists)
				throw new SportNotFoundException();

			var subsTypeExists = await _subscriptionTypeRepository.CheckIfSubscriptionTypeExists(request.SubsTypeId, cancellationToken);
			if (!subsTypeExists)
				throw new SubscriptionTypeNotFoundException();

			cancellationToken.ThrowIfCancellationRequested();

			if (request.NewPrice <= 0)
				throw new InvalidPriceException();

			var sportPrice = await _sportPriceRepository
				.GetByKeyAsync(request.BranchId, request.SportId, request.SubsTypeId, cancellationToken)
				?? throw new SportPriceNotFoundException();

			sportPrice.Price = request.NewPrice;

			await _sportPriceRepository.UpdateAsync(sportPrice, cancellationToken);

			return Result<decimal>.Success(sportPrice.Price, _operationType);
		}
	
}

}
