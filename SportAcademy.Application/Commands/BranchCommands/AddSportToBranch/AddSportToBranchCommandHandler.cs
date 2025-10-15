using MediatR;
using SportAcademy.Application.Interfaces;
using SportAcademy.Application.Services;
using SportAcademy.Domain.Entities;
using SportAcademy.Domain.Enums;
using SportAcademy.Domain.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SportAcademy.Application.Commands.BranchCommands.AddSportToBranch
{
    public class AddSportToBranchCommandHandler : IRequestHandler<AddSportToBranchCommand, Result<string>>
    {
        private readonly IBranchRepository _branchRepository;
        private readonly ISportRepository _sportRepository;
        private readonly ISportBranchRepository _sportBranchRepository;
        private readonly string _operationType = OperationType.Add.ToString();

        public AddSportToBranchCommandHandler(
            IBranchRepository branchRepository,
            ISportRepository sportRepository,
            ISportBranchRepository sportBranchRepository)
        {
            _branchRepository = branchRepository;
            _sportRepository = sportRepository;
            _sportBranchRepository = sportBranchRepository;
        }

        public async Task<Result<string>> Handle(AddSportToBranchCommand request, CancellationToken cancellationToken)
        {
            var branch = await _branchRepository.GetByIdAsync(request.BranchId, cancellationToken)
                ?? throw new IdNotFoundException(nameof(Branch), request.BranchId.ToString());

            var sport = await _sportRepository.GetByIdAsync(request.SportId, cancellationToken)
                ?? throw new IdNotFoundException(nameof(Sport), request.SportId.ToString());

            var exists = await _sportBranchRepository.ExistsAsync(request.SportId, request.BranchId, cancellationToken);
            if (exists)
                throw new ConflictException(nameof(Sport), nameof(Branch));

            var sportBranch = new SportBranch
            {
                SportId = request.SportId,
                BranchId = request.BranchId
            };

            await _sportBranchRepository.AddAsync(sportBranch, cancellationToken);

            return Result<string>.Success("Sport added to branch successfully.", _operationType);
        }
    }
}
