using MediatR;
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
        var tenant = await _tenantRepository.GetByIdAsync(request.TenantId, ct);
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

        // The plan itself carries no other configuration this system enforces - its features are
        // the "specs" a plan actually grants (see SubscriptionPlanFeature). A downgrade must
        // revoke access to anything the tenant no longer has entitlement to, not just relabel
        // the plan while every previously-enabled feature keeps working. An upgrade only makes
        // new features *available* (allowedFeatureIds is recomputed live from the plan on every
        // read) rather than force-enabling them - that stays the tenant's own opt-in choice, the
        // same self-service model UpdateTenantFeatureCommandHandler already enforces.
        // A SuperAdmin-locked feature is excluded here explicitly (not left to
        // BulkUpdateFeaturesAsync's own defensive skip) - a forced decision overrides plan
        // membership entirely, same as it overrides the tenant's own choice, and that must be
        // this handler's own guarantee, not an incidental side effect of its collaborator.
        var newPlanFeatureIds = await _tenantRepository.GetPlanFeaturesAsync(plan.Id, ct);
        var currentFeatures = await _tenantRepository.GetTenantFeaturesAsync(request.TenantId, ct);
        var revocations = currentFeatures
            .Where(f => f.IsEnabled && !f.LockedBySuperAdmin && !newPlanFeatureIds.Contains(f.FeatureId))
            .ToDictionary(f => f.FeatureId, _ => false);

        if (revocations.Count > 0)
            await _tenantRepository.BulkUpdateFeaturesAsync(request.TenantId, revocations, "PlanChange", ct);

        await _unitOfWork.SaveChangesAsync(ct);

        return Result.Success(_operation, $"Tenant plan changed to {plan.Name}.");
    }
}
