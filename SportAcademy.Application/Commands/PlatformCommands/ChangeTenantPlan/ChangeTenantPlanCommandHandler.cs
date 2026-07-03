using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Contract;
using SportAcademy.Domain.Entities.Tenants;
using SportAcademy.Domain.Enums;

namespace SportAcademy.Application.Commands.PlatformCommands.ChangeTenantPlan;

public class ChangeTenantPlanCommandHandler : IRequestHandler<ChangeTenantPlanCommand, Result>
{
    private readonly ITenantRepository _tenantRepository;
    private readonly IBaseRepository<SubscriptionPlan, int> _planRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly string _operation = OperationType.Update.ToString();

    public ChangeTenantPlanCommandHandler(
        ITenantRepository tenantRepository,
        IBaseRepository<SubscriptionPlan, int> planRepository,
        IUnitOfWork unitOfWork)
    {
        _tenantRepository = tenantRepository;
        _planRepository = planRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(ChangeTenantPlanCommand request, CancellationToken ct)
    {
        var tenant = await _tenantRepository.GetByIdAsync(request.TenantId, ct);
        if (tenant is null)
            return Result.Failure(_operation, "Tenant not found.", 404);

        var plan = await _planRepository.GetByIdAsync(request.NewPlanId, ct);
        if (plan is null)
            return Result.Failure(_operation, "Subscription plan not found.", 404);

        if (tenant.Subscription is null)
            return Result.Failure(_operation, "Tenant subscription not found.", 404);

        if (tenant.Subscription.SubscriptionPlanId == request.NewPlanId)
            return Result.Failure(_operation, "Tenant is already on this plan.", 400);

        tenant.Subscription.SubscriptionPlanId = plan.Id;
        await _unitOfWork.SaveChangesAsync(ct);

        return Result.Success(_operation, $"Tenant plan changed to {plan.Name}.");
    }
}
