using MediatR;
using SportAcademy.Application.Common.Features;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Contract;
using SportAcademy.Domain.Entities.Tenants;
using SportAcademy.Domain.Enums;

namespace SportAcademy.Application.Commands.PlatformCommands.ChangeTenantPlan;

public class ChangeTenantPlanCommandHandler : IRequestHandler<ChangeTenantPlanCommand, Result>
{
    private readonly ITenantRepository _tenantRepository;
    private readonly IBaseRepository<SubscriptionPlan, int> _planRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly string _operation = OperationType.Update.ToString();

    public ChangeTenantPlanCommandHandler(
        ITenantRepository tenantRepository,
        IBaseRepository<SubscriptionPlan, int> planRepository,
        IUnitOfWork unitOfWork)
    {
        _tenantRepository = tenantRepository;
        _planRepository = planRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(ChangeTenantPlanCommand request, CancellationToken ct)
    {
        var tenant = await _tenantRepository.GetDetailByIdAsync(request.TenantId, ct);
        if (tenant is null)
            return Result.Failure(_operation, "Tenant not found.", 404);

        var plan = await _planRepository.GetByIdAsync(request.NewPlanId, ct);
        if (plan is null)
            return Result.Failure(_operation, "Subscription plan not found.", 404);

        if (tenant.Subscription is null)
            return Result.Failure(_operation, "Tenant subscription not found.", 404);

        if (tenant.Subscription.SubscriptionPlanId == request.NewPlanId)
            return Result.Failure(_operation, "Tenant is already on this plan.", 400);

        request.ResolvedBeforeState = new { tenant.Subscription.SubscriptionPlanId };
        tenant.Subscription.SubscriptionPlanId = plan.Id;

        // A plan change reconciles the tenant's TenantFeature rows to exactly match the new
        // plan's feature set: anything the new plan grants that isn't already enabled gets
        // enabled, anything it no longer grants that's currently enabled gets disabled. A
        // SuperAdmin-locked feature (TenantFeature.LockedBySuperAdmin) is untouched either way -
        // a forced decision overrides plan membership entirely. See PlanFeatureReconciler for
        // the shared rule, also used when a plan's own feature set is edited
        // (UpdatePlanFeaturesCommandHandler).
        var newPlanFeatureIds = await _tenantRepository.GetPlanFeaturesAsync(plan.Id, ct);
        var currentFeatures = await _tenantRepository.GetTenantFeaturesAsync(request.TenantId, ct);
        var featureNameById = (await _tenantRepository.GetAllFeaturesAsync(ct)).ToDictionary(f => f.Id, f => f.Name);
        var updates = PlanFeatureReconciler.ComputeUpdates(currentFeatures, newPlanFeatureIds, featureNameById);

        if (updates.Count > 0)
            await _tenantRepository.BulkUpdateFeaturesAsync(request.TenantId, updates, "PlanChange", ct);

        await _unitOfWork.SaveChangesAsync(ct);

        return Result.Success(_operation, $"Tenant plan changed to {plan.Name}.");
    }
}
