using AutoMapper;
using MediatR;
using SportAcademy.Application.Interfaces;
using SportAcademy.Application.Services;
using SportAcademy.Domain.Entities;
using SportAcademy.Domain.Exceptions.BranchExceptions;
using SportAcademy.Domain.Exceptions.UserExceptions;
using SportAcademy.Domain.ValueObjects;

namespace SportAcademy.Application.Commands.BranchCommands.CreateBranch
{
	public class CreateBranchCommandHandler : IRequestHandler<CreateBranchCommand, Result<int>>
	{
		private readonly IBranchRepository _branchRepository;
		private readonly IMapper _mapper;
		private readonly string _operationType = Domain.Enums.OperationType.Add.ToString();
		public CreateBranchCommandHandler(
			IBranchRepository branchRepository,
			IMapper mapper)
		{
			_branchRepository = branchRepository;
			_mapper = mapper;
		}
		public async Task<Result<int>> Handle(CreateBranchCommand request, CancellationToken cancellationToken)
		{
			var branch = _mapper.Map<Branch>(request)
				?? throw new AutoMapperMappingException("Error occurred while mapping.");

			branch.Coordinate = Coordinate.Create(request.CoX, request.CoY);

            var emailExists = await _branchRepository.IsEmailExistAsync(branch.Email, cancellationToken);
			if (emailExists)
				throw new EmailExistException();
			
			var isCoordinatesUsed = await _branchRepository.IsCoordinatesExistAsync(branch.Coordinate, cancellationToken);
			if (isCoordinatesUsed)
				throw new CoordinateExistException();

			var phoneExists = await _branchRepository.IsPhoneNumberExistAsync(branch.PhoneNumber, cancellationToken);
			if (phoneExists)
				throw new PhoneExistException();

			branch.IsActive = true;

			cancellationToken.ThrowIfCancellationRequested();

			await _branchRepository.AddAsync(branch, cancellationToken);
			return Result<int>.Success(branch.Id, _operationType);
		}
	}
}
