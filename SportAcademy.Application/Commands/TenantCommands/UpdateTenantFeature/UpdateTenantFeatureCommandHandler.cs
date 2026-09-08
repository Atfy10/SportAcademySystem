using MediatR;
using SportAcademy.Application.Common.Features;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.DTOs.TenantDtos;
using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Contract;
using SportAcademy.Domain.Entities.Tenants;
using SportAcademy.Domain.Enums;

namespace SportAcademy.Application.Commands.TenantCommands.UpdateTenantFeature;

public class UpdateTenantFeatureCommandHandler : IRequestHandler<UpdateTenantFeatureCommand, Result>
{
    private readonly ITenantRepository _tenantRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IUserContextService _userContext;
    private readonly string _operation = OperationType.Update.ToString();

    public UpdateTenantFeatureCommandHandler(
        ITenantRepository tenantRepository,
        IUnitOfWork unitOfWork,
        IUserContextService userContext)
    {
        _tenantRepository = tenantRepository;
        _unitOfWork = unitOfWork;
        _userContext = userContext;
    }

    public async Task<Result> Handle(UpdateTenantFeatureCommand request, CancellationToken ct)
    {
        var tenantId = _userContext.TenantId;
        if (tenantId is null)
            return Result.Failure(_operation, "Tenant ID is not available.", 400);

        var tenant = await _tenantRepository.GetDetailByIdAsync(tenantId.Value, ct);
        if (tenant is null)
            return Result.Failure(_operation, "Tenant not found.", 404);

        var planId = tenant.Subscription?.SubscriptionPlanId;
        var allowedFeatureIds = planId.HasValue
            ? await _tenantRepository.GetPlanFeaturesAsync(planId.Value, ct)
            : new List<Guid>();

        if (!allowedFeatureIds.Contains(request.FeatureId))
            return Result.Failure(_operation, "Feature is not available in your subscription plan.", 403);

        var tenantFeature = await _tenantRepository.GetTenantFeatureAsync(tenantId.Value, request.FeatureId, ct);

        if (tenantFeature is not null)
        {
            if (tenantFeature.LockedBySuperAdmin)
                return Result.Failure(_operation, "This feature is managed by your plan and can't be changed here.", 403);

            if (tenantFeature.IsEnabled == request.IsEnabled)
                return Result.Failure(_operation, $"Feature is already {(request.IsEnabled ? "enabled" : "disabled")}.", 400);
        }

        // Every feature toggle must keep TenantFeature.IsEnabled dependency-closed - self-service
        // never cascades (see FeatureDependencyPolicy), so a violation here just blocks the
        // request with a message naming what's in the way.
        var allFeatures = await _tenantRepository.GetAllFeaturesAsync(ct);
        var featureNameById = allFeatures.ToDictionary(f => f.Id, f => f.Name);
        if (featureNameById.TryGetValue(request.FeatureId, out var featureName))
        {
            var tenantFeatures = await _tenantRepository.GetTenantFeaturesAsync(tenantId.Value, ct);
            var currentlyEnabledNames = tenantFeatures
                .Where(tf => tf.IsEnabled && tf.FeatureId != request.FeatureId && featureNameById.ContainsKey(tf.FeatureId))
                .Select(tf => featureNameById[tf.FeatureId])
                .ToHashSet();

            var dependencyError = request.IsEnabled
                ? FeatureDependencyPolicy.ValidateEnable(featureName, currentlyEnabledNames)
                : FeatureDependencyPolicy.ValidateDisable(featureName, currentlyEnabledNames);

            if (dependencyError is not null)
                return Result.Failure(_operation, dependencyError, 409);
        }

        if (tenantFeature is not null)
        {
            tenantFeature.IsEnabled = request.IsEnabled;
            tenantFeature.EnabledAt = DateTime.UtcNow;
            tenantFeature.EnabledBy = "TenantAdmin";
        }
        else if (request.IsEnabled)
        {
            await _tenantRepository.AddTenantFeatureAsync(new TenantFeature
            {
                TenantId = tenantId.Value,
                FeatureId = request.FeatureId,
                IsEnabled = true,
                EnabledAt = DateTime.UtcNow,
                EnabledBy = "TenantAdmin"
            }, ct);
        }

        await _unitOfWork.SaveChangesAsync(ct);

        return Result.Success(_operation, $"Feature {(request.IsEnabled ? "enabled" : "disabled")} successfully.");
    }
}
