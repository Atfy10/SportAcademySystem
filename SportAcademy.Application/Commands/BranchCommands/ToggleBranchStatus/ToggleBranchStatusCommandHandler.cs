using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Enums;
using SportAcademy.Domain.Exceptions.BranchExceptions;

namespace SportAcademy.Application.Commands.BranchCommands.ToggleBranchStatus;

public class ToggleBranchStatusCommandHandler : IRequestHandler<ToggleBranchStatusCommand, Result<bool>>
{
    private readonly IBranchRepository _branchRepository;
    private readonly string _operation = OperationType.Update.ToString();

    public ToggleBranchStatusCommandHandler(IBranchRepository branchRepository)
    {
        _branchRepository = branchRepository;
    }

    public async Task<Result<bool>> Handle(ToggleBranchStatusCommand request, CancellationToken cancellationToken)
    {
        var branch = await _branchRepository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new BranchNotFoundException(request.Id.ToString());

        branch.ToggleStatus();

        await _branchRepository.UpdateAsync(branch, cancellationToken);

        return Result<bool>.Success(branch.IsActive, _operation);
    }
}
