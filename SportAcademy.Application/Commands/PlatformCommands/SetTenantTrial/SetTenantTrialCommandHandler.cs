using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Domain.Contract;
using SportAcademy.Domain.Enums;

namespace SportAcademy.Application.Commands.PlatformCommands.SetTenantTrial;

public class SetTenantTrialCommandHandler : IRequestHandler<SetTenantTrialCommand, Result>
{
    private readonly ITenantRepository _tenantRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly string _operation = OperationType.Update.ToString();

    public SetTenantTrialCommandHandler(
        ITenantRepository tenantRepository,
        IUnitOfWork unitOfWork)
    {
        _tenantRepository = tenantRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(SetTenantTrialCommand request, CancellationToken ct)
    {
        var tenant = await _tenantRepository.GetByIdAsync(request.TenantId, ct);
        if (tenant is null)
            return Result.Failure(_operation, "Tenant not found.", 404);

        if (tenant.Subscription is null)
            return Result.Failure(_operation, "Tenant subscription not found.", 404);

        if (tenant.Subscription.IsTrial)
            return Result.Failure(_operation, "Tenant is already on a trial.", 400);

        tenant.Subscription.IsTrial = true;
        tenant.Subscription.EndsAt = DateTime.UtcNow.AddDays(14);
        tenant.Subscription.AutoRenew = false;
        await _unitOfWork.SaveChangesAsync(ct);

        return Result.Success(_operation, "Tenant set to trial for 14 days.");
    }
}
