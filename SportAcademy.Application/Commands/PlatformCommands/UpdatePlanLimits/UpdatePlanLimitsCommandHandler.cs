using MediatR;
using SportAcademy.Application.Common.Limits;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Contract;
using SportAcademy.Domain.Entities.Tenants;
using SportAcademy.Domain.Enums;

namespace SportAcademy.Application.Commands.PlatformCommands.UpdatePlanLimits;

public class UpdatePlanLimitsCommandHandler : IRequestHandler<UpdatePlanLimitsCommand, Result>
{
    private readonly ITenantRepository _tenantRepository;
    private readonly IBaseRepository<SubscriptionPlan, int> _planRepository;
    private readonly ILimitReconciliationService _limitReconciliationService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly string _operation = OperationType.Update.ToString();

    public UpdatePlanLimitsCommandHandler(
        ITenantRepository tenantRepository,
        IBaseRepository<SubscriptionPlan, int> planRepository,
        ILimitReconciliationService limitReconciliationService,
        IUnitOfWork unitOfWork)
    {
        _tenantRepository = tenantRepository;
        _planRepository = planRepository;
        _limitReconciliationService = limitReconciliationService;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(UpdatePlanLimitsCommand request, CancellationToken ct)
    {
        var plan = await _planRepository.GetByIdAsync(request.PlanId, ct);
        if (plan is null)
            return Result.Failure(_operation, "Subscription plan not found.", 404);

        var previousLimits = await _tenantRepository.GetPlanLimitsAsync(request.PlanId, ct);
        request.ResolvedBeforeState = new
        {
            PreviousLimits = previousLimits.ToDictionary(l => l.ResourceKey, l => l.MaxCount)
        };

        await _tenantRepository.ReplacePlanLimitsAsync(request.PlanId, request.Limits, ct);

        // Must commit before the reconciliation loop below reads limits back - ReplacePlanLimitsAsync
        // only stages a RemoveRange+AddRange, and a fresh query in the same DbContext would
        // otherwise still return the OLD rows (a pending-Delete entity's property values are
        // unaffected until the DELETE actually lands) while the newly-added rows wouldn't appear
        // at all (they don't exist in the DB yet). Without this, every ReconcileAsync call below
        // would evaluate against the limits this request is replacing, not the new ones.
        await _unitOfWork.SaveChangesAsync(ct);

        // Retroactive reconciliation: every tenant currently on this plan is checked against
        // the new limits right away - same "safe on every startup, applies immediately" rule
        // UpdatePlanFeaturesCommandHandler already follows for feature edits. ReconcileAsync
        // looks at each tenant's current status itself, so raising this plan's limits can resolve
        // an existing lock just as effectively as opening a new one on a lowered limit.
        var tenantIds = await _tenantRepository.GetTenantIdsSubscribedToPlanAsync(request.PlanId, ct);
        var lockedCount = 0;
        var resolvedCount = 0;
        foreach (var tenantId in tenantIds)
        {
            var outcome = await _limitReconciliationService.ReconcileAsync(tenantId, ct);
            if (outcome == LimitReconciliationOutcome.Opened)
                lockedCount++;
            else if (outcome == LimitReconciliationOutcome.Resolved)
                resolvedCount++;
        }

        var lockedNote = lockedCount > 0
            ? $" {lockedCount} tenant(s) are now over the new limits and must complete a forced selection."
            : "";
        var resolvedNote = resolvedCount > 0
            ? $" {resolvedCount} previously-locked tenant(s) are now within limits again and active."
            : "";
        return Result.Success(_operation, $"Updated {plan.Name} plan limits and checked {tenantIds.Count} tenant(s).{lockedNote}{resolvedNote}");
    }
}
