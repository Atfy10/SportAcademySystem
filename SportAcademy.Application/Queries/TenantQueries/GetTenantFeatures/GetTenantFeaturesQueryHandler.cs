using MediatR;
using SportAcademy.Application.Common.Features;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.DTOs.TenantDtos;
using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Contract;
using SportAcademy.Domain.Enums;

namespace SportAcademy.Application.Queries.TenantQueries.GetTenantFeatures;

public class GetTenantFeaturesQueryHandler : IRequestHandler<GetTenantFeaturesQuery, Result<TenantFeaturesListDto>>
{
    private readonly ITenantRepository _tenantRepository;
    private readonly IUserContextService _userContext;
    private readonly string _operation = OperationType.Get.ToString();

    public GetTenantFeaturesQueryHandler(ITenantRepository tenantRepository, IUserContextService userContext)
    {
        _tenantRepository = tenantRepository;
        _userContext = userContext;
    }

    public async Task<Result<TenantFeaturesListDto>> Handle(GetTenantFeaturesQuery request, CancellationToken ct)
    {
        var tenantId = _userContext.TenantId;
        if (tenantId is null)
            return Result<TenantFeaturesListDto>.Failure(_operation, "Tenant ID is not available.", 400);

        var tenant = await _tenantRepository.GetDetailByIdAsync(tenantId.Value, ct);
        if (tenant is null)
            return Result<TenantFeaturesListDto>.Failure(_operation, "Tenant not found.", 404);

        var planId = tenant.Subscription?.SubscriptionPlanId;
        var allowedFeatureIds = planId.HasValue
            ? await _tenantRepository.GetPlanFeaturesAsync(planId.Value, ct)
            : new List<Guid>();

        var allFeatures = await _tenantRepository.GetAllFeaturesAsync(ct);
        var tenantFeatures = await _tenantRepository.GetTenantFeaturesAsync(tenantId.Value, ct);

        var features = allFeatures.Select(f =>
        {
            var tf = tenantFeatures.FirstOrDefault(x => x.FeatureId == f.Id);
            return new TenantFeatureDto
            {
                FeatureId = f.Id,
                Name = f.Name,
                DisplayName = f.DisplayName,
                Description = f.Description,
                Category = FeatureCategories.For(f.Name),
                IsEnabled = tf?.IsEnabled ?? false,
                CanToggle = allowedFeatureIds.Contains(f.Id) && !(tf?.LockedBySuperAdmin ?? false) && !FeatureDependencies.IsProtected(f.Name) && f.IsImplemented,
                LockedBySuperAdmin = tf?.LockedBySuperAdmin ?? false,
                EnabledAt = tf?.EnabledAt,
                DependsOn = FeatureDependencies.GetPrerequisites(f.Name),
                RequiredBy = FeatureDependencies.GetDependents(f.Name),
                IsProtected = FeatureDependencies.IsProtected(f.Name),
                IsImplemented = f.IsImplemented
            };
        }).ToList();

        var byCategory = features
            .GroupBy(f => f.Category)
            .ToDictionary(g => g.Key, g => g.ToList());

        var result = new TenantFeaturesListDto
        {
            Features = features,
            ByCategory = byCategory
        };

        return Result<TenantFeaturesListDto>.Success(result, _operation);
    }
}
