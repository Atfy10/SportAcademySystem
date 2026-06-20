using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.Interfaces;
using SportAcademy.Application.Mappings;
using SportAcademy.Domain.Enums;
using SportAcademy.Domain.Exceptions.BranchExceptions;
using SportAcademy.Domain.Exceptions.UserExceptions;

namespace SportAcademy.Application.Commands.BranchCommands.CreateBranch
{
    public class CreateBranchCommandHandler : IRequestHandler<CreateBranchCommand, Result<int>>
    {
        private readonly IBranchRepository _branchRepository;
        private readonly string _operationType = OperationType.Add.ToString();

        public CreateBranchCommandHandler(IBranchRepository branchRepository)
        {
            _branchRepository = branchRepository;
        }

        public async Task<Result<int>> Handle(CreateBranchCommand request, CancellationToken cancellationToken)
        {
            var branch = request.ToBranch();

            var emailExists = await _branchRepository.IsEmailExistAsync(branch.Email ?? "", cancellationToken);
            if (emailExists)
                throw new EmailExistException();

            var isCoordinatesUsed = await _branchRepository.IsCoordinatesExistAsync(branch.CoX, branch.CoY, cancellationToken);
            if (isCoordinatesUsed)
                throw new CoordinateExistException();

            var phoneExists = await _branchRepository.IsPhoneNumberExistAsync(branch.PhoneNumber, cancellationToken);
            if (phoneExists)
                throw new PhoneExistException();

            cancellationToken.ThrowIfCancellationRequested();

            await _branchRepository.AddAsync(branch, cancellationToken);
            return Result<int>.Success(branch.Id, _operationType);
        }
    }
}
