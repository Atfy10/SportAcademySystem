using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.DTOs.BranchDtos;
using SportAcademy.Application.Interfaces;
using SportAcademy.Application.Mappings;
using SportAcademy.Domain.Enums;
using SportAcademy.Domain.Exceptions.BranchExceptions;
using SportAcademy.Domain.Exceptions.UserExceptions;

namespace SportAcademy.Application.Commands.BranchCommands.UpdateBranch
{
    public class UpdateBranchCommandHandler : IRequestHandler<UpdateBranchCommand, Result<BranchDto>>
    {
        private readonly IBranchRepository _branchRepository;
        private readonly string _operationType = OperationType.Update.ToString();

        public UpdateBranchCommandHandler(IBranchRepository branchRepository)
        {
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

            var coordinatesChanged = (request.CoX != branch.CoX) || (request.CoY != branch.CoY);
            if (coordinatesChanged)
            {
                var coordinatesExist = await _branchRepository.IsCoordinatesExistAsync(request.CoX!, request.CoY!, cancellationToken);
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

            branch.ApplyUpdate(request);

            cancellationToken.ThrowIfCancellationRequested();

            await _branchRepository.UpdateAsync(branch, cancellationToken);

            var branchDto = branch.ToDto();

            return Result<BranchDto>.Success(branchDto, _operationType);
        }
    }
}
