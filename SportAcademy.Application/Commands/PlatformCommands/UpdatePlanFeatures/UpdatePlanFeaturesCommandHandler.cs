using MediatR;
using SportAcademy.Application.Common.Features;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Contract;
using SportAcademy.Domain.Entities.Tenants;
using SportAcademy.Domain.Enums;

namespace SportAcademy.Application.Commands.PlatformCommands.UpdatePlanFeatures;

public class UpdatePlanFeaturesCommandHandler : IRequestHandler<UpdatePlanFeaturesCommand, Result>
{
    private readonly ITenantRepository _tenantRepository;
    private readonly IBaseRepository<SubscriptionPlan, int> _planRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly string _operation = OperationType.Update.ToString();

    public UpdatePlanFeaturesCommandHandler(
        ITenantRepository tenantRepository,
        IBaseRepository<SubscriptionPlan, int> planRepository,
        IUnitOfWork unitOfWork)
    {
        _tenantRepository = tenantRepository;
        _planRepository = planRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(UpdatePlanFeaturesCommand request, CancellationToken ct)
    {
        var plan = await _planRepository.GetByIdAsync(request.PlanId, ct);
        if (plan is null)
            return Result.Failure(_operation, "Subscription plan not found.", 404);

        var allFeatureIds = (await _tenantRepository.GetAllFeaturesAsync(ct)).Select(f => f.Id).ToHashSet();
        if (request.FeatureIds.Any(id => !allFeatureIds.Contains(id)))
            return Result.Failure(_operation, "Unknown feature id(s).", 400);

        request.ResolvedBeforeState = new
        {
            PreviousFeatureIds = await _tenantRepository.GetPlanFeaturesAsync(request.PlanId, ct)
        };

        await _tenantRepository.ReplacePlanFeaturesAsync(request.PlanId, request.FeatureIds, ct);

        // Retroactive reconciliation: every tenant currently on this plan gets its TenantFeature
        // rows reconciled against the new feature set right away, using the same symmetric rule
        // ChangeTenantPlanCommandHandler uses when a tenant moves between plans - see
        // PlanFeatureReconciler.
        var tenantIds = await _tenantRepository.GetTenantIdsSubscribedToPlanAsync(request.PlanId, ct);
        foreach (var tenantId in tenantIds)
        {
            var currentFeatures = await _tenantRepository.GetTenantFeaturesAsync(tenantId, ct);
            var updates = PlanFeatureReconciler.ComputeUpdates(currentFeatures, request.FeatureIds);

            if (updates.Count > 0)
                await _tenantRepository.BulkUpdateFeaturesAsync(tenantId, updates, "PlanFeaturesUpdated", ct);
        }

        await _unitOfWork.SaveChangesAsync(ct);

        return Result.Success(_operation, $"Updated {plan.Name} plan features and reconciled {tenantIds.Count} tenant(s).");
    }
}
