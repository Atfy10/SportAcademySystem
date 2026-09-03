using MediatR;
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
    private readonly string _operation = OperationType.Update.ToString();

    public UpdateCoachBranchesCommandHandler(
        ICoachRepository coachRepository,
        ICoachBranchAccessRepository coachBranchAccessRepository)
    {
        _coachRepository = coachRepository;
        _coachBranchAccessRepository = coachBranchAccessRepository;
    }

    public async Task<Result<bool>> Handle(UpdateCoachBranchesCommand request, CancellationToken ct)
    {
        var coach = await _coachRepository.GetByIdAsync(request.CoachId, ct)
            ?? throw new IdNotFoundException(nameof(Coach), request.CoachId);

        if (request.BranchIds.Count == 0)
            return Result<bool>.Failure(_operation, "A coach must be authorized for at least one branch.", 400);

        var access = request.BranchIds
            .Distinct()
            .Select(branchId => new CoachBranchAccess { BranchId = branchId });

        await _coachBranchAccessRepository.ReplaceForCoachAsync(coach.EmployeeId, coach.TenantId, access.ToList(), ct);

        return Result<bool>.Success(true, _operation);
    }
}
