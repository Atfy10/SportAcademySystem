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

        // An override can move the effective limit in either direction - re-evaluate regardless
        // (EvaluateAsync is a no-op if the tenant still has headroom everywhere).
        var reconciliationOpened = await _limitReconciliationService.EvaluateAsync(request.TenantId, ct);

        await _unitOfWork.SaveChangesAsync(ct);

        var limitDescription = request.MaxCount?.ToString() ?? "unlimited";
        return Result.Success(_operation, reconciliationOpened
            ? $"{request.ResourceKey} limit set to {limitDescription}. The tenant is now over this limit and must complete a forced selection before continuing."
            : $"{request.ResourceKey} limit set to {limitDescription}.");
    }
}
