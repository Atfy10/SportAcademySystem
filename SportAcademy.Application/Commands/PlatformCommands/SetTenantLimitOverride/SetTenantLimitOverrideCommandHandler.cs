using MediatR;
using SportAcademy.Application.Common.Limits;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Contract;
using SportAcademy.Domain.Entities.Tenants;
using SportAcademy.Domain.Enums;

namespace SportAcademy.Application.Commands.PlatformCommands.SetTenantLimitOverride;

public class SetTenantLimitOverrideCommandHandler : IRequestHandler<SetTenantLimitOverrideCommand, Result>
{
    private readonly ITenantRepository _tenantRepository;
    private readonly ILimitReconciliationService _limitReconciliationService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly string _operation = OperationType.Update.ToString();

    public SetTenantLimitOverrideCommandHandler(
        ITenantRepository tenantRepository,
        ILimitReconciliationService limitReconciliationService,
        IUnitOfWork unitOfWork)
    {
        _tenantRepository = tenantRepository;
        _limitReconciliationService = limitReconciliationService;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(SetTenantLimitOverrideCommand request, CancellationToken ct)
    {
        var tenant = await _tenantRepository.GetByIdAsync(request.TenantId, ct);
        if (tenant is null)
            return Result.Failure(_operation, "Tenant not found.", 404);

        var existing = await _tenantRepository.GetTenantLimitOverrideAsync(request.TenantId, request.ResourceKey, ct);
        request.ResolvedBeforeState = existing is null
            ? null
            : new { existing.MaxCount, existing.Reason };

        await _tenantRepository.SetTenantLimitOverrideAsync(new TenantLimitOverride
        {
            TenantId = request.TenantId,
            ResourceKey = request.ResourceKey,
            MaxCount = request.MaxCount,
            SetBy = "SuperAdmin",
            SetAt = DateTime.UtcNow,
            Reason = request.Reason,
        }, ct);

        // Must commit before ReconcileAsync reads overrides back - for a brand-new override (no
        // prior row for this tenant/resource), SetTenantLimitOverrideAsync only stages an AddAsync,
        // and a newly-added, unsaved entity never appears in a fresh query's results (the SQL
        // executes against the database, which doesn't have the row yet). Without this, setting a
        // tenant's first-ever override on a resource would reconcile against the plan's own limit
        // instead of the override just requested.
        await _unitOfWork.SaveChangesAsync(ct);

        // An override can move the effective limit in either direction - ReconcileAsync looks at
        // the tenant's current status itself, so it doesn't matter which.
        var outcome = await _limitReconciliationService.ReconcileAsync(request.TenantId, ct);

        var limitDescription = request.MaxCount?.ToString() ?? "unlimited";
        var message = outcome switch
        {
            LimitReconciliationOutcome.Opened =>
                $"{request.ResourceKey} limit set to {limitDescription}. The tenant is now over this limit and must complete a forced selection before continuing.",
            LimitReconciliationOutcome.Resolved =>
                $"{request.ResourceKey} limit set to {limitDescription}. The tenant is now within its limits again and is active.",
            _ => $"{request.ResourceKey} limit set to {limitDescription}.",
        };

        return Result.Success(_operation, message);
    }
}
