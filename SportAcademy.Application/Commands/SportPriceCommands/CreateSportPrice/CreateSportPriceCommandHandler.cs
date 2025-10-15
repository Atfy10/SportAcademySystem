using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SportAcademy.Application.Interfaces;
using SportAcademy.Application.Services;
using SportAcademy.Domain.Entities;
using SportAcademy.Domain.Enums;
using SportAcademy.Domain.Exceptions;

namespace SportAcademy.Application.Commands.SportPriceCommands.CreateSportPrice
{
	public class CreateSportPriceCommandHandler : IRequestHandler<CreateSportPriceCommand, Result<decimal>>
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
		public async Task<Result<decimal>> Handle(CreateSportPriceCommand request, CancellationToken cancellationToken)
		{
			var keyExists = await _sportPriceRepository
				.CheckIfKeyExists(request.BranchId, request.SportId, request.SubsTypeId, cancellationToken);
			if (keyExists)
				throw new SportPriceExistsException();

			var branchExists = await _branchRepository.IsBranchExistAsync(request.BranchId, cancellationToken);
			if (!branchExists)
				throw new BranchNotFoundException();

			var sportExists = await _sportRepository.CheckIfSportExists(request.SportId, cancellationToken);
			if (!sportExists)
				throw new SportNotFoundException();

			var subsTypeExists = await _subscriptionTypeRepository.CheckIfSubscriptionTypeExists(request.SubsTypeId, cancellationToken);
			if (!subsTypeExists)
				throw new SubscriptionTypeNotFoundException();

			cancellationToken.ThrowIfCancellationRequested();

			if (request.Price <= 0)
				throw new InvalidPriceException();

			var sportPrice = _mapper.Map<SportPrice>(request)
				?? throw new AutoMapperMappingException("Error occurred while mapping.");

			await _sportPriceRepository.AddAsync(sportPrice, cancellationToken);

			return Result<decimal>.Success(sportPrice.Price, _operationType);
		}
	}
}


