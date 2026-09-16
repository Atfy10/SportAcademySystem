using MediatR;
using SportAcademy.Application.Common.Limits;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Enums;
using SportAcademy.Domain.Exceptions.BranchExceptions;

namespace SportAcademy.Application.Commands.BranchCommands.ToggleBranchStatus;

public class ToggleBranchStatusCommandHandler : IRequestHandler<ToggleBranchStatusCommand, Result<bool>>
{
    private readonly IBranchRepository _branchRepository;
    private readonly IUserContextService _userContext;
    private readonly IEffectiveLimitService _limitService;
    private readonly string _operation = OperationType.Update.ToString();

    public ToggleBranchStatusCommandHandler(
        IBranchRepository branchRepository, IUserContextService userContext, IEffectiveLimitService limitService)
    {
        _branchRepository = branchRepository;
        _userContext = userContext;
        _limitService = limitService;
    }

    public async Task<Result<bool>> Handle(ToggleBranchStatusCommand request, CancellationToken cancellationToken)
    {
        var branch = await _branchRepository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new BranchNotFoundException(request.Id.ToString());

        // Only reactivating (false -> true) consumes a slot - deactivating always frees one and
        // must never be blocked by the same cap that's stopping new branches. Un-gated, a
        // tenant could simply re-enable a branch it was forced to deactivate to bypass the
        // downgrade-reconciliation wizard's whole point (see PLAN_LIMITS_DESIGN.md R4).
        if (!branch.IsActive && _userContext.TenantId is { } tenantId)
        {
            var limit = await _limitService.GetAsync(tenantId, LimitedResources.Branches, cancellationToken);
            if (!limit.HasHeadroom)
                return Result<bool>.Failure(_operation, limit.ToMessage(), 403, limit.ToErrorDictionary());
        }

        var newStatus = await _branchRepository.ToggleIsActiveAsync(request.Id, cancellationToken);
        return Result<bool>.Success(newStatus, _operation);
    }
}
