using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.DTOs.TenantDtos;
using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Contract;
using SportAcademy.Domain.Enums;

namespace SportAcademy.Application.Commands.TenantCommands.BulkUpdateTenantFeatures;

public class BulkUpdateTenantFeaturesCommandHandler : IRequestHandler<BulkUpdateTenantFeaturesCommand, Result>
{
    private readonly ITenantRepository _tenantRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IUserContextService _userContext;
    private readonly string _operation = OperationType.Update.ToString();

    public BulkUpdateTenantFeaturesCommandHandler(
        ITenantRepository tenantRepository,
        IUnitOfWork unitOfWork,
        IUserContextService userContext)
    {
        _tenantRepository = tenantRepository;
        _unitOfWork = unitOfWork;
        _userContext = userContext;
    }

    public async Task<Result> Handle(BulkUpdateTenantFeaturesCommand request, CancellationToken ct)
    {
        var tenantId = _userContext.TenantId;
        if (tenantId is null)
            return Result.Failure(_operation, "Tenant ID is not available.", 400);

        var tenant = await _tenantRepository.GetDetailByIdAsync(tenantId.Value, ct);
        if (tenant is null)
            return Result.Failure(_operation, "Tenant not found.", 404);

        var planId = tenant.Subscription?.SubscriptionPlanId;
        var allowedFeatureIds = planId.HasValue
            ? await _tenantRepository.GetPlanFeaturesAsync(planId.Value, ct)
            : new List<Guid>();

        var validUpdates = request.FeatureStates
            .Where(kvp => allowedFeatureIds.Contains(kvp.Key))
            .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

        await _tenantRepository.BulkUpdateFeaturesAsync(tenantId.Value, validUpdates, "TenantAdmin", ct);
        await _unitOfWork.SaveChangesAsync(ct);

        return Result.Success(_operation, "Features updated successfully.");
    }
}
