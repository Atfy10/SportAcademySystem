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

        // Retroactive reconciliation: every tenant currently on this plan is checked against
        // the new limits right away - same "safe on every startup, applies immediately" rule
        // UpdatePlanFeaturesCommandHandler already follows for feature edits.
        var tenantIds = await _tenantRepository.GetTenantIdsSubscribedToPlanAsync(request.PlanId, ct);
        var lockedCount = 0;
        foreach (var tenantId in tenantIds)
        {
            if (await _limitReconciliationService.EvaluateAsync(tenantId, ct))
                lockedCount++;
        }

        await _unitOfWork.SaveChangesAsync(ct);

        var lockedNote = lockedCount > 0
            ? $" {lockedCount} tenant(s) are now over the new limits and must complete a forced selection."
            : "";
        return Result.Success(_operation, $"Updated {plan.Name} plan limits and checked {tenantIds.Count} tenant(s).{lockedNote}");
    }
}
