using AutoMapper;
using MediatR;
using SportAcademy.Application.Interfaces;
using SportAcademy.Application.Services;
using SportAcademy.Domain.Enums;
using SportAcademy.Domain.Exceptions.BranchExceptions;
using SportAcademy.Domain.Exceptions.SharedExceptions;
using SportAcademy.Domain.Exceptions.SportExceptions;
using SportAcademy.Domain.Exceptions.SubscriptonExceptions;

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
			var keyExists = await _sportPriceRepository.IsExistAsync(request.BranchId,
				request.SportId, request.SubsTypeId, cancellationToken);
			if (!keyExists)
				throw new SportPriceNotFoundException($"{request.BranchId}, {request.SportId}, {request.SubsTypeId}");

			var branchExists = await _branchRepository.IsExistAsync(request.BranchId, cancellationToken);
			if (!branchExists)
				throw new BranchNotFoundException($"{request.BranchId}");

			var sportExists = await _sportRepository.IsExistAsync(request.SportId, cancellationToken);
			if (!sportExists)
				throw new SportNotFoundException($"{request.SportId}");

			var subsTypeExists = await _subscriptionTypeRepository.IsExistAsync(request.SubsTypeId, cancellationToken);
			if (!subsTypeExists)
				throw new SubscriptionTypeNotFoundException($"{request.SubsTypeId}");

			cancellationToken.ThrowIfCancellationRequested();

			if (request.NewPrice <= 0)
				throw new InvalidPriceException();

			var sportPrice = await _sportPriceRepository
				.GetByKeyAsync(request.BranchId, request.SportId, request.SubsTypeId, cancellationToken)
				?? throw new SportPriceNotFoundException($"{request.BranchId}, {request.SportId}, {request.SubsTypeId}");

			sportPrice.Price = request.NewPrice;

			await _sportPriceRepository.UpdateAsync(sportPrice, cancellationToken);

			return Result<decimal>.Success(sportPrice.Price, _operationType);
		}
	
}

}
