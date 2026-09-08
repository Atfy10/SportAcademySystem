using MediatR;
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

        var tenantFeature = await _tenantRepository.GetTenantFeatureAsync(request.TenantId, request.FeatureId, ct);

        if (tenantFeature is not null)
        {
            // Both parts of the request's intent must already match, not just IsEnabled - a
            // request that only changes Lock (e.g. releasing a previously-forced feature back to
            // the tenant, value unchanged) must still go through.
            if (tenantFeature.IsEnabled == request.IsEnabled && tenantFeature.LockedBySuperAdmin == request.Lock)
                return Result.Failure(_operation, $"Feature is already {(request.IsEnabled ? "enabled" : "disabled")} and {(request.Lock ? "locked" : "unlocked")}.", 400);

            request.ResolvedBeforeState = new { tenantFeature.IsEnabled, tenantFeature.LockedBySuperAdmin };

            tenantFeature.IsEnabled = request.IsEnabled;
            tenantFeature.EnabledAt = DateTime.UtcNow;
            tenantFeature.EnabledBy = "SuperAdmin";
            tenantFeature.LockedBySuperAdmin = request.Lock;
        }
        else
        {
            await _tenantRepository.AddTenantFeatureAsync(new TenantFeature
            {
                TenantId = request.TenantId,
                FeatureId = request.FeatureId,
                IsEnabled = request.IsEnabled,
                EnabledAt = DateTime.UtcNow,
                EnabledBy = "SuperAdmin",
                LockedBySuperAdmin = request.Lock
            }, ct);
        }

        await _unitOfWork.SaveChangesAsync(ct);

        var lockNote = request.Lock ? " and locked" : "";
        return Result.Success(_operation, $"Feature {(request.IsEnabled ? "enabled" : "disabled")}{lockNote} successfully.");
    }
}
