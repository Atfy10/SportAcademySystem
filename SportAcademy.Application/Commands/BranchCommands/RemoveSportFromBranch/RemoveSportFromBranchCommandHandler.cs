using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Entities;
using SportAcademy.Domain.Enums;
using SportAcademy.Domain.Exceptions.SportExceptions;

namespace SportAcademy.Application.Commands.BranchCommands.RemoveSportFromBranch
{
    public class RemoveSportFromBranchCommandHandler : IRequestHandler<RemoveSportFromBranchCommand, Result<string>>
    {
        private readonly ISportBranchRepository _sportBranchRepository;
        private readonly string _operationType = OperationType.Delete.ToString();

        public RemoveSportFromBranchCommandHandler(ISportBranchRepository sportBranchRepository)
        {
            _sportBranchRepository = sportBranchRepository;
        }

        public async Task<Result<string>> Handle(RemoveSportFromBranchCommand request, CancellationToken cancellationToken)
        {
            var exists = await _sportBranchRepository.IsExistAsync(request.SportId, request.BranchId, cancellationToken);
            if (!exists)
                throw new SportBranchNotFoundException();

            var sportBranch = new SportBranch
            {
                SportId = request.SportId,
                BranchId = request.BranchId
            };

            await _sportBranchRepository.DeleteAsync(sportBranch, cancellationToken);

            return Result<string>.Success("Sport removed from branch successfully.", _operationType);
        }
    }
}
