using AutoMapper;
using MediatR;
using SportAcademy.Application.DTOs.SportPriceDtos;
using SportAcademy.Application.Interfaces;
using SportAcademy.Application.Services;
using SportAcademy.Domain.Entities;
using SportAcademy.Domain.Enums;
using SportAcademy.Domain.Exceptions.BranchExceptions;
using SportAcademy.Domain.Exceptions.SharedExceptions;
using SportAcademy.Domain.Exceptions.SportExceptions;
using SportAcademy.Domain.Exceptions.SubscriptonExceptions;

namespace SportAcademy.Application.Commands.SportPriceCommands.CreateSportPrice
{
	public class CreateSportPriceCommandHandler : IRequestHandler<CreateSportPriceCommand, Result<SportPriceBranchDto>>
	{
		private readonly ISportPriceRepository _sportPriceRepository;
		private readonly ISportRepository _sportRepository;
		private readonly IBranchRepository _branchRepository;
		private readonly ISubscriptionTypeRepository _subscriptionTypeRepository;
		private readonly string _operationType = OperationType.Add.ToString();
		private readonly IMapper _mapper;

		public CreateSportPriceCommandHandler(
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
		public async Task<Result<SportPriceBranchDto>> Handle(CreateSportPriceCommand request, CancellationToken cancellationToken)
		{
			var keyExists = await _sportPriceRepository.IsExistAsync(request.BranchId, 
				request.SportId, request.SubsTypeId, cancellationToken);
			if (keyExists)
				throw new SportPriceExistsException();

			var branchExists = await _branchRepository.IsExistAsync(request.BranchId,
				cancellationToken);
			if (!branchExists)
				throw new BranchNotFoundException($"{request.BranchId}");

			var sportExists = await _sportRepository.IsExistAsync(request.SportId,
				cancellationToken);
			if (!sportExists)
				throw new SportNotFoundException($"{request.SportId}");

			var subsTypeExists = await _subscriptionTypeRepository.IsExistAsync(
				request.SubsTypeId, cancellationToken);
			if (!subsTypeExists)
				throw new SubscriptionTypeNotFoundException($"{request.SubsTypeId}");

			cancellationToken.ThrowIfCancellationRequested();

			if (request.Price <= 0)
				throw new InvalidPriceException();

			var sportPrice = _mapper.Map<SportPrice>(request)
				?? throw new AutoMapperMappingException("Error occurred while mapping.");

            await _sportPriceRepository.AddAsync(sportPrice, cancellationToken);

			cancellationToken.ThrowIfCancellationRequested();

			sportPrice = await _sportPriceRepository.GetByKeyWithIncludesAsync(request.BranchId, request.SportId, request.SubsTypeId);

            cancellationToken.ThrowIfCancellationRequested();

            var dto = _mapper.Map<SportPriceBranchDto>(sportPrice);
            return Result<SportPriceBranchDto>.Success(dto, _operationType);
		}
	}
}


