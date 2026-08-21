using AutoMapper;
using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.DTOs.BranchDtos;
using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Enums;
using SportAcademy.Domain.Exceptions.BranchExceptions;
using SportAcademy.Domain.Exceptions.UserExceptions;

namespace SportAcademy.Application.Commands.BranchCommands.UpdateBranch
{
	public class UpdateBranchCommandHandler : IRequestHandler<UpdateBranchCommand, Result<BranchDto>>
	{
		private readonly IMapper _mapper;
		private readonly IBranchRepository _branchRepository;
		private readonly string _operationType = OperationType.Update.ToString();

		public UpdateBranchCommandHandler(
			IMapper mapper,
			IBranchRepository branchRepository)
		{
			_mapper = mapper;
			_branchRepository = branchRepository;
		}
		public async Task<Result<BranchDto>> Handle(UpdateBranchCommand request, CancellationToken cancellationToken)
		{
			var branch = await _branchRepository.GetByIdAsync(request.Id, cancellationToken)
				?? throw new BranchNotFoundException($"{request.Id}");

			if (!string.IsNullOrEmpty(request.Email) && request.Email != branch.Email)
			{
				var emailExists = await _branchRepository.IsEmailExistAsync(request.Email, cancellationToken);
				if (emailExists)
					throw new EmailExistException();
			}

			var newCoX = string.IsNullOrWhiteSpace(request.CoX) ? null : request.CoX;
			var newCoY = string.IsNullOrWhiteSpace(request.CoY) ? null : request.CoY;
			var coordinatesChanged = (newCoX != branch.CoX) || (newCoY != branch.CoY);
			if (coordinatesChanged && newCoX is not null && newCoY is not null)
			{
				var coordinatesExist = await _branchRepository.IsCoordinatesExistAsync(newCoX, newCoY, cancellationToken);
				if (coordinatesExist)
					throw new CoordinateExistException();
			}

			var isPhoneChanged = !string.IsNullOrEmpty(request.PhoneNumber) 
				&& request.PhoneNumber != branch.PhoneNumber;
            if (isPhoneChanged)
			{
				var phoneExists = await _branchRepository.IsPhoneNumberExistAsync(request.PhoneNumber, cancellationToken);
				if (phoneExists)
					throw new PhoneExistException();
			}

			_mapper.Map(request, branch);
			branch.CoX = newCoX;
			branch.CoY = newCoY;

			cancellationToken.ThrowIfCancellationRequested();

			await _branchRepository.UpdateAsync(branch, cancellationToken);

			var branchDto = _mapper.Map<BranchDto>(branch)
				?? throw new AutoMapperMappingException("Error occurred while mapping.");

			return Result<BranchDto>.Success(branchDto, _operationType);
		}
	}
}
