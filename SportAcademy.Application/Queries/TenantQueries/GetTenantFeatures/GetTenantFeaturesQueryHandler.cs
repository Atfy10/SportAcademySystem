using MediatR;
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

    private static readonly Dictionary<string, string> FeatureCategoryMap = new()
    {
        ["user-management"] = "Management",
        ["role-management"] = "Management",
        ["tenant-settings"] = "Management",
        ["branch-management"] = "Management",
        ["trainee-management"] = "Management",
        ["employee-management"] = "Management",
        ["coach-management"] = "Management",
        ["sport-management"] = "Management",
        ["subscription-plan"] = "Management",
        ["pricing-management"] = "Management",
        ["payment-processing"] = "Management",
        ["profile-mgmt"] = "Management",
        ["api-access"] = "Management",
        ["backup-restore"] = "Management",
        ["system-settings"] = "Management",
        ["trainee-codes"] = "Management",
        ["group-management"] = "Operations",
        ["schedule-management"] = "Operations",
        ["attendance-tracking"] = "Operations",
        ["enrollment-management"] = "Operations",
        ["family-management"] = "Operations",
        ["nationality-categories"] = "Operations",
        ["session-management"] = "Operations",
        ["financial-reports"] = "Finance",
        ["discount-offers"] = "Finance",
        ["notifications"] = "Communication",
        ["chat-system"] = "Communication",
        ["trainee-reports"] = "Analytics",
        ["coach-reports"] = "Analytics",
        ["operational-reports"] = "Analytics",
        ["attendance-reports"] = "Analytics",
        ["video-analysis"] = "Analytics",
        ["health-test-mgmt"] = "Analytics",
        ["ai-assistant"] = "Analytics",
        ["audit-trail"] = "Security",
    };

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
                Category = FeatureCategoryMap.GetValueOrDefault(f.Name, "Management"),
                IsEnabled = tf?.IsEnabled ?? false,
                CanToggle = allowedFeatureIds.Contains(f.Id),
                EnabledAt = tf?.EnabledAt
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
