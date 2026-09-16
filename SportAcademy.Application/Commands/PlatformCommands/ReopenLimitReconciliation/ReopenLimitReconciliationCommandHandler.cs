using MediatR;
using SportAcademy.Application.Common.Limits;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Contract;
using SportAcademy.Domain.Entities.Tenants;
using SportAcademy.Domain.Enums;

namespace SportAcademy.Application.Commands.PlatformCommands.ReopenLimitReconciliation;

public class ReopenLimitReconciliationCommandHandler : IRequestHandler<ReopenLimitReconciliationCommand, Result>
{
    private readonly ITenantRepository _tenantRepository;
    private readonly ILimitReconciliationService _limitReconciliationService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly string _operation = OperationType.Update.ToString();

    public ReopenLimitReconciliationCommandHandler(
        ITenantRepository tenantRepository,
        ILimitReconciliationService limitReconciliationService,
        IUnitOfWork unitOfWork)
    {
        _tenantRepository = tenantRepository;
        _limitReconciliationService = limitReconciliationService;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(ReopenLimitReconciliationCommand request, CancellationToken ct)
    {
        var tenant = await _tenantRepository.GetByIdAsync(request.TenantId, ct);
        if (tenant is null)
            return Result.Failure(_operation, "Tenant not found.", 404);

        if (!TenantStatusPolicy.CanTransition(tenant.Status, TenantStatus.PendingLimitSelection))
            return Result.Failure(_operation,
                $"Cannot reopen a limit selection window for a tenant that is {tenant.Status}.", 400);

        request.ResolvedBeforeState = new { tenant.Status };

        await _limitReconciliationService.ReopenAsync(request.TenantId, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        return Result.Success(_operation, "Limit selection window reopened for 7 days.");
    }
}
