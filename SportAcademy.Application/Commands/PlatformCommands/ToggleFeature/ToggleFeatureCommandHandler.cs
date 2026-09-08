using MediatR;
using SportAcademy.Application.Common.Features;
using SportAcademy.Application.Common.Result;
using SportAcademy.Domain.Contract;
using SportAcademy.Domain.Entities.Tenants;
using SportAcademy.Domain.Enums;

namespace SportAcademy.Application.Commands.PlatformCommands.ToggleFeature;

public class ToggleFeatureCommandHandler : IRequestHandler<ToggleFeatureCommand, Result>
{
    private readonly ITenantRepository _tenantRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly string _operation = OperationType.Update.ToString();

    public ToggleFeatureCommandHandler(
        ITenantRepository tenantRepository,
        IUnitOfWork unitOfWork)
    {
        _tenantRepository = tenantRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(ToggleFeatureCommand request, CancellationToken ct)
    {
        var tenant = await _tenantRepository.GetByIdAsync(request.TenantId, ct);
        if (tenant is null)
            return Result.Failure(_operation, "Tenant not found.", 404);

        var allFeatures = await _tenantRepository.GetAllFeaturesAsync(ct);
        var featureNameById = allFeatures.ToDictionary(f => f.Id, f => f.Name);
        var featureIdByName = allFeatures.ToDictionary(f => f.Name, f => f.Id);

        if (!featureNameById.TryGetValue(request.FeatureId, out var featureName))
            return Result.Failure(_operation, "Unknown feature.", 404);

        var tenantFeature = await _tenantRepository.GetTenantFeatureAsync(request.TenantId, request.FeatureId, ct);

        if (tenantFeature is not null)
        {
            // Both parts of the request's intent must already match, not just IsEnabled - a
            // request that only changes Lock (e.g. releasing a previously-forced feature back to
            // the tenant, value unchanged) must still go through.
            if (tenantFeature.IsEnabled == request.IsEnabled && tenantFeature.LockedBySuperAdmin == request.Lock)
                return Result.Failure(_operation, $"Feature is already {(request.IsEnabled ? "enabled" : "disabled")} and {(request.Lock ? "locked" : "unlocked")}.", 400);
        }

        // ResolveCascade below only ever checks the protected status of features it cascades
        // *onto* (the dependents/prerequisites of request.FeatureId) - it never sees
        // request.FeatureId itself, so the feature being directly toggled needs this explicit
        // guard. Unlike self-service (FeatureDependencyPolicy.ValidateProtectedDisable, an
        // unconditional block), a SuperAdmin is allowed to disable a protected feature - they can
        // always re-enable it - but only after explicitly confirming; the first attempt is
        // rejected with a warning the frontend turns into a confirmation dialog, and the code
        // "PROTECTED_FEATURE_CONFIRM_REQUIRED" is what it keys off of to know to do that.
        if (!request.IsEnabled && !request.ConfirmProtectedDisable && FeatureDependencies.IsProtected(featureName))
        {
            return Result.Failure(
                _operation,
                $"'{featureName}' is a protected platform capability. Disabling it may limit the tenant's " +
                "own ability to recover access - you can always re-enable it yourself. Confirm to proceed.",
                409,
                new Dictionary<string, string[]> { ["code"] = ["PROTECTED_FEATURE_CONFIRM_REQUIRED"] });
        }

        // A cascade is only needed when the value itself is changing - a Lock-only change (or
        // creating a row for the first time at a given value) doesn't ripple onto anything else.
        FeatureCascadeResult? cascade = null;
        if (tenantFeature is null || tenantFeature.IsEnabled != request.IsEnabled)
        {
            var currentTenantFeatures = await _tenantRepository.GetTenantFeaturesAsync(request.TenantId, ct);
            var currentByName = allFeatures.ToDictionary(
                f => f.Name,
                f =>
                {
                    var existing = currentTenantFeatures.FirstOrDefault(tf => tf.FeatureId == f.Id);
                    return new FeatureRuntimeState(existing?.IsEnabled ?? false, existing?.LockedBySuperAdmin ?? false);
                });

            cascade = FeatureDependencyPolicy.ResolveCascade(featureName, request.IsEnabled, currentByName);

            if (cascade.IsBlocked)
                return Result.Failure(
                    _operation,
                    $"Can't {(request.IsEnabled ? "enable" : "disable")} '{featureName}': " +
                    $"{string.Join(", ", cascade.LockedConflicts)} locked by a SuperAdmin in a conflicting state - " +
                    "unlock it first.",
                    409);
        }

        if (tenantFeature is not null)
            request.ResolvedBeforeState = new { tenantFeature.IsEnabled, tenantFeature.LockedBySuperAdmin };

        await ApplyToggleAsync(request.TenantId, request.FeatureId, tenantFeature, request.IsEnabled, request.Lock, "SuperAdmin", ct);

        var cascadeCount = 0;
        if (cascade is not null)
        {
            foreach (var affectedName in cascade.AffectedFeatureNames)
            {
                if (!featureIdByName.TryGetValue(affectedName, out var affectedId))
                    continue;

                var affectedFeature = await _tenantRepository.GetTenantFeatureAsync(request.TenantId, affectedId, ct);
                await ApplyToggleAsync(request.TenantId, affectedId, affectedFeature, request.IsEnabled, lockValue: false, "SuperAdmin (cascade)", ct);
                cascadeCount++;
            }
        }

        await _unitOfWork.SaveChangesAsync(ct);

        var lockNote = request.Lock ? " and locked" : "";
        var cascadeNote = cascadeCount > 0
            ? $" ({cascadeCount} dependent feature(s) also {(request.IsEnabled ? "enabled" : "disabled")})"
            : "";
        return Result.Success(_operation, $"Feature {(request.IsEnabled ? "enabled" : "disabled")}{lockNote} successfully.{cascadeNote}");
    }

    private async Task ApplyToggleAsync(
        Guid tenantId, Guid featureId, TenantFeature? existing, bool isEnabled, bool lockValue, string enabledBy, CancellationToken ct)
    {
        if (existing is not null)
        {
            existing.IsEnabled = isEnabled;
            existing.EnabledAt = DateTime.UtcNow;
            existing.EnabledBy = enabledBy;
            existing.LockedBySuperAdmin = lockValue;
        }
        else
        {
            await _tenantRepository.AddTenantFeatureAsync(new TenantFeature
            {
                TenantId = tenantId,
                FeatureId = featureId,
                IsEnabled = isEnabled,
                EnabledAt = DateTime.UtcNow,
                EnabledBy = enabledBy,
                LockedBySuperAdmin = lockValue
            }, ct);
        }
    }
}
