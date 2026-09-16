using MediatR;
using SportAcademy.Application.Common.Limits;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Contract;
using SportAcademy.Domain.Enums;

namespace SportAcademy.Application.Commands.PlatformCommands.RemoveTenantLimitOverride;

public class RemoveTenantLimitOverrideCommandHandler : IRequestHandler<RemoveTenantLimitOverrideCommand, Result>
{
    private readonly ITenantRepository _tenantRepository;
    private readonly ILimitReconciliationService _limitReconciliationService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly string _operation = OperationType.Update.ToString();

    public RemoveTenantLimitOverrideCommandHandler(
        ITenantRepository tenantRepository,
        ILimitReconciliationService limitReconciliationService,
        IUnitOfWork unitOfWork)
    {
        _tenantRepository = tenantRepository;
        _limitReconciliationService = limitReconciliationService;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(RemoveTenantLimitOverrideCommand request, CancellationToken ct)
    {
        var existing = await _tenantRepository.GetTenantLimitOverrideAsync(request.TenantId, request.ResourceKey, ct);
        if (existing is null)
            return Result.Failure(_operation, "No override exists for this tenant/resource.", 404);

        request.ResolvedBeforeState = new { existing.MaxCount, existing.Reason };

        await _tenantRepository.RemoveTenantLimitOverrideAsync(request.TenantId, request.ResourceKey, ct);

        // Must commit before ReconcileAsync reads overrides back - RemoveTenantLimitOverrideAsync
        // only stages a Remove (marks the entity Deleted), and a pending-Delete entity's property
        // values are still returned as-is by a fresh query in the same DbContext until the DELETE
        // actually lands (the mirror image of the "new override" staleness case - see
        // SetTenantLimitOverrideCommandHandler). Without this, ReconcileAsync would still see the
        // override that's being removed, evaluating against the wrong ceiling.
        await _unitOfWork.SaveChangesAsync(ct);

        // Falling back to the plan's own limit can itself put the tenant over cap (the override
        // being removed may have been raising the ceiling, not lowering it) - or resolve an
        // existing lock if it was the override itself holding the tenant back. ReconcileAsync
        // looks at the tenant's current status itself, so it doesn't matter which.
        var outcome = await _limitReconciliationService.ReconcileAsync(request.TenantId, ct);

        var message = outcome switch
        {
            LimitReconciliationOutcome.Opened =>
                $"{request.ResourceKey} limit reset to the plan default. The tenant is now over this limit and must complete a forced selection before continuing.",
            LimitReconciliationOutcome.Resolved =>
                $"{request.ResourceKey} limit reset to the plan default. The tenant is now within its limits again and is active.",
            _ => $"{request.ResourceKey} limit reset to the plan default.",
        };

        return Result.Success(_operation, message);
    }
}
