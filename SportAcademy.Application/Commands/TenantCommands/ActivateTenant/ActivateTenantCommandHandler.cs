using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Contract;
using SportAcademy.Domain.Entities.Tenants;
using SportAcademy.Domain.Enums;
using SportAcademy.Domain.Events;

namespace SportAcademy.Application.Commands.TenantCommands.ActivateTenant;

public class ActivateTenantCommandHandler : IRequestHandler<ActivateTenantCommand, Result>
{
    private readonly IBaseRepository<Tenant, Guid> _tenantRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMediator _mediator;
    private readonly string _operation = OperationType.Update.ToString();

    public ActivateTenantCommandHandler(
        IBaseRepository<Tenant, Guid> tenantRepository,
        IUnitOfWork unitOfWork,
        IMediator mediator)
    {
        _tenantRepository = tenantRepository;
        _unitOfWork = unitOfWork;
        _mediator = mediator;
    }

    public async Task<Result> Handle(ActivateTenantCommand request, CancellationToken ct)
    {
        var tenant = await _tenantRepository.GetByIdAsync(request.TenantId, ct);
        if (tenant is null)
            return Result.Failure(_operation, "Tenant not found.", 404);

        if (tenant.Status is not TenantStatus.PendingSetup)
            return Result.Success(_operation, "Tenant is already active.");

        tenant.Status = TenantStatus.Active;
        await _unitOfWork.SaveChangesAsync(ct);

        await _mediator.Publish(new TenantActivatedEvent(tenant.Id), ct);

        return Result.Success(_operation, "Tenant activated successfully.");
    }
}
