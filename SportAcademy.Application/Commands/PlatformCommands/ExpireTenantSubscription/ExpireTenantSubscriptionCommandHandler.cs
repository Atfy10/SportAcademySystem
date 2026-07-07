using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Domain.Contract;
using SportAcademy.Domain.Enums;

namespace SportAcademy.Application.Commands.PlatformCommands.ExpireTenantSubscription;

public class ExpireTenantSubscriptionCommandHandler : IRequestHandler<ExpireTenantSubscriptionCommand, Result>
{
    private readonly ITenantRepository _tenantRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly string _operation = OperationType.Update.ToString();

    public ExpireTenantSubscriptionCommandHandler(
        ITenantRepository tenantRepository,
        IUnitOfWork unitOfWork)
    {
        _tenantRepository = tenantRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(ExpireTenantSubscriptionCommand request, CancellationToken ct)
    {
        var tenant = await _tenantRepository.GetByIdAsync(request.TenantId, ct);
        if (tenant is null)
            return Result.Failure(_operation, "Tenant not found.", 404);

        if (tenant.Subscription is null)
            return Result.Failure(_operation, "Tenant subscription not found.", 404);

        tenant.Subscription.EndsAt = DateTime.UtcNow;
        await _unitOfWork.SaveChangesAsync(ct);

        return Result.Success(_operation, "Subscription expired manually.");
    }
}
