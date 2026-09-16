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

        // Falling back to the plan's own limit can itself put the tenant over cap (the override
        // being removed may have been raising the ceiling, not lowering it).
        var reconciliationOpened = await _limitReconciliationService.EvaluateAsync(request.TenantId, ct);

        await _unitOfWork.SaveChangesAsync(ct);

        return Result.Success(_operation, reconciliationOpened
            ? $"{request.ResourceKey} limit reset to the plan default. The tenant is now over this limit and must complete a forced selection before continuing."
            : $"{request.ResourceKey} limit reset to the plan default.");
    }
}
