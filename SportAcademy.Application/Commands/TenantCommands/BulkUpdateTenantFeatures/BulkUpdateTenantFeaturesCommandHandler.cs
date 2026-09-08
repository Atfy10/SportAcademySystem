using MediatR;
using SportAcademy.Application.Common.Features;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.DTOs.TenantDtos;
using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Contract;
using SportAcademy.Domain.Enums;

namespace SportAcademy.Application.Commands.TenantCommands.BulkUpdateTenantFeatures;

public class BulkUpdateTenantFeaturesCommandHandler : IRequestHandler<BulkUpdateTenantFeaturesCommand, Result>
{
    private readonly ITenantRepository _tenantRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IUserContextService _userContext;
    private readonly string _operation = OperationType.Update.ToString();

    public BulkUpdateTenantFeaturesCommandHandler(
        ITenantRepository tenantRepository,
        IUnitOfWork unitOfWork,
        IUserContextService userContext)
    {
        _tenantRepository = tenantRepository;
        _unitOfWork = unitOfWork;
        _userContext = userContext;
    }

    public async Task<Result> Handle(BulkUpdateTenantFeaturesCommand request, CancellationToken ct)
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

        var allFeatures = await _tenantRepository.GetAllFeaturesAsync(ct);
        var currentTenantFeatures = await _tenantRepository.GetTenantFeaturesAsync(tenantId.Value, ct);
        var currentById = currentTenantFeatures.ToDictionary(tf => tf.FeatureId);

        // Same skip rules BulkUpdateFeaturesAsync itself applies (plan membership, SuperAdmin
        // lock) - computed here too so the dependency check below validates the end-state that
        // will actually be persisted, not the raw requested payload.
        var validUpdates = request.FeatureStates
            .Where(kvp => allowedFeatureIds.Contains(kvp.Key))
            .Where(kvp => !(currentById.TryGetValue(kvp.Key, out var existing) && existing.LockedBySuperAdmin))
            .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

        // Validate the full post-save state (unchanged features plus this payload's changes) is
        // dependency-closed - self-service never cascades, see FeatureDependencyPolicy.
        var endStateByName = new Dictionary<string, bool>();
        foreach (var feature in allFeatures)
        {
            var isEnabled = validUpdates.TryGetValue(feature.Id, out var requested)
                ? requested
                : currentById.TryGetValue(feature.Id, out var existing) && existing.IsEnabled;
            endStateByName[feature.Name] = isEnabled;
        }

        var dependencyErrors = FeatureDependencyPolicy.ValidateEndState(endStateByName);
        if (dependencyErrors.Count > 0)
            return Result.Failure(_operation, string.Join(" ", dependencyErrors), 409);

        await _tenantRepository.BulkUpdateFeaturesAsync(tenantId.Value, validUpdates, "TenantAdmin", ct);
        await _unitOfWork.SaveChangesAsync(ct);

        return Result.Success(_operation, "Features updated successfully.");
    }
}
