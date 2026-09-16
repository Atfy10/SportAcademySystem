using MediatR;
using SportAcademy.Application.Common;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Entities;
using SportAcademy.Domain.Enums;
using SportAcademy.Domain.Exceptions.BaseExceptions;

namespace SportAcademy.Application.Commands.CoachCommands.UpdateCoachBranches;

public class UpdateCoachBranchesCommandHandler : IRequestHandler<UpdateCoachBranchesCommand, Result<bool>>
{
    private readonly ICoachRepository _coachRepository;
    private readonly ICoachBranchAccessRepository _coachBranchAccessRepository;
    private readonly IBranchRepository _branchRepository;
    private readonly string _operation = OperationType.Update.ToString();

    public UpdateCoachBranchesCommandHandler(
        ICoachRepository coachRepository,
        ICoachBranchAccessRepository coachBranchAccessRepository,
        IBranchRepository branchRepository)
    {
        _coachRepository = coachRepository;
        _coachBranchAccessRepository = coachBranchAccessRepository;
        _branchRepository = branchRepository;
    }

    public async Task<Result<bool>> Handle(UpdateCoachBranchesCommand request, CancellationToken ct)
    {
        var coach = await _coachRepository.GetByIdAsync(request.CoachId, ct)
            ?? throw new IdNotFoundException(nameof(Coach), request.CoachId);

        if (request.Branches.Count == 0)
            return Result<bool>.Failure(_operation, "A coach must be authorized for at least one branch.", 400);

        // See NewlyAddedBranchGuard: this is a full-replace, not a diff - re-submitting a branch
        // the coach was already authorized for (even one since deactivated) must not break.
        var existingBranchIds = (await _coachBranchAccessRepository.GetForCoachAsync(coach.EmployeeId, ct))
            .Select(a => a.BranchId);

        var inactiveNewBranchId = await NewlyAddedBranchGuard.FindInactiveNewlyAddedBranchAsync(
            _branchRepository, request.Branches.Select(b => b.BranchId), existingBranchIds, ct);
        if (inactiveNewBranchId is not null)
            return Result<bool>.Failure(_operation, "This branch has been deactivated and can no longer be used.", 400);

        var access = request.Branches
            .DistinctBy(b => b.BranchId)
            .Select(b => new CoachBranchAccess { BranchId = b.BranchId, Salary = b.Salary });

        await _coachBranchAccessRepository.ReplaceForCoachAsync(coach.EmployeeId, coach.TenantId, access.ToList(), ct);

        return Result<bool>.Success(true, _operation);
    }
}
