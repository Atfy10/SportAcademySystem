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
        var newStatus = await _branchRepository.ToggleIsActiveAsync(request.Id, cancellationToken);
        if (!newStatus)
            throw new BranchNotFoundException(request.Id.ToString());

        return Result<bool>.Success(newStatus, _operation);
    }
}
