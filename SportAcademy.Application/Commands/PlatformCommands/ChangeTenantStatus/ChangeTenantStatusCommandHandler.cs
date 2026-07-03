using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Domain.Contract;
using SportAcademy.Domain.Enums;

namespace SportAcademy.Application.Commands.PlatformCommands.ChangeTenantStatus;

public class ChangeTenantStatusCommandHandler : IRequestHandler<ChangeTenantStatusCommand, Result>
{
    private readonly ITenantRepository _tenantRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly string _operation = OperationType.Update.ToString();

    public ChangeTenantStatusCommandHandler(
        ITenantRepository tenantRepository,
        IUnitOfWork unitOfWork)
    {
        _tenantRepository = tenantRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(ChangeTenantStatusCommand request, CancellationToken ct)
    {
        var tenant = await _tenantRepository.GetByIdAsync(request.TenantId, ct);
        if (tenant is null)
            return Result.Failure(_operation, "Tenant not found.", 404);

        if (tenant.Status == request.NewStatus)
            return Result.Failure(_operation, $"Tenant is already {request.NewStatus}.", 400);

        if (!IsValidTransition(tenant.Status, request.NewStatus))
            return Result.Failure(_operation,
                $"Cannot transition from {tenant.Status} to {request.NewStatus}.", 400);

        tenant.Status = request.NewStatus;
        await _unitOfWork.SaveChangesAsync(ct);

        return Result.Success(_operation, $"Tenant status changed to {request.NewStatus}.");
    }

    private static bool IsValidTransition(TenantStatus current, TenantStatus next)
    {
        return (current, next) switch
        {
            (TenantStatus.PendingSetup, TenantStatus.Suspended) => true,
            (TenantStatus.Active, TenantStatus.Suspended) => true,
            (TenantStatus.Active, TenantStatus.Inactive) => true,
            (TenantStatus.Suspended, TenantStatus.Active) => true,
            (TenantStatus.Inactive, TenantStatus.Active) => true,
            (TenantStatus.Suspended, TenantStatus.Archived) => true,
            (TenantStatus.Inactive, TenantStatus.Archived) => true,
            _ => false
        };
    }
}
