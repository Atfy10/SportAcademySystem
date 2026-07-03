using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Domain.Contract;
using SportAcademy.Domain.Enums;

namespace SportAcademy.Application.Commands.PlatformCommands.ArchiveTenant;

public class ArchiveTenantCommandHandler : IRequestHandler<ArchiveTenantCommand, Result>
{
    private readonly ITenantRepository _tenantRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly string _operation = OperationType.Update.ToString();

    public ArchiveTenantCommandHandler(
        ITenantRepository tenantRepository,
        IUnitOfWork unitOfWork)
    {
        _tenantRepository = tenantRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(ArchiveTenantCommand request, CancellationToken ct)
    {
        var tenant = await _tenantRepository.GetByIdAsync(request.TenantId, ct);
        if (tenant is null)
            return Result.Failure(_operation, "Tenant not found.", 404);

        if (tenant.Status == TenantStatus.Archived)
            return Result.Failure(_operation, "Tenant is already archived.", 400);

        if (tenant.Status == TenantStatus.PendingSetup)
            return Result.Failure(_operation, "Cannot archive a tenant that has not been activated. Suspend it first.", 400);

        tenant.Status = TenantStatus.Archived;
        await _unitOfWork.SaveChangesAsync(ct);

        return Result.Success(_operation, "Tenant archived successfully.");
    }
}
