using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Domain.Contract;
using SportAcademy.Domain.Enums;

namespace SportAcademy.Application.Commands.PlatformCommands.ExtendTenantSubscription;

public class ExtendTenantSubscriptionCommandHandler : IRequestHandler<ExtendTenantSubscriptionCommand, Result>
{
    private readonly ITenantRepository _tenantRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly string _operation = OperationType.Update.ToString();

    public ExtendTenantSubscriptionCommandHandler(
        ITenantRepository tenantRepository,
        IUnitOfWork unitOfWork)
    {
        _tenantRepository = tenantRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(ExtendTenantSubscriptionCommand request, CancellationToken ct)
    {
        if (request.Days <= 0)
            return Result.Failure(_operation, "Days must be greater than zero.", 400);

        var tenant = await _tenantRepository.GetByIdAsync(request.TenantId, ct);
        if (tenant is null)
            return Result.Failure(_operation, "Tenant not found.", 404);

        if (tenant.Subscription is null)
            return Result.Failure(_operation, "Tenant subscription not found.", 404);

        var baseDate = tenant.Subscription.EndsAt > DateTime.UtcNow
            ? tenant.Subscription.EndsAt
            : DateTime.UtcNow;

        tenant.Subscription.EndsAt = baseDate.AddDays(request.Days);
        await _unitOfWork.SaveChangesAsync(ct);

        return Result.Success(_operation, $"Subscription extended by {request.Days} days.");
    }
}
