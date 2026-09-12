using MediatR;
using SportAcademy.Application.Common.Features;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.DTOs.PublicDtos;
using SportAcademy.Domain.Contract;
using SportAcademy.Domain.Enums;

namespace SportAcademy.Application.Queries.PublicQueries.GetBundleFeatures;

public class GetBundleFeaturesQueryHandler : IRequestHandler<GetBundleFeaturesQuery, Result<List<BundleFeatureDto>>>
{
    private readonly ITenantRepository _tenantRepository;
    private readonly string _operation = OperationType.Get.ToString();

    public GetBundleFeaturesQueryHandler(ITenantRepository tenantRepository)
    {
        _tenantRepository = tenantRepository;
    }

    public async Task<Result<List<BundleFeatureDto>>> Handle(GetBundleFeaturesQuery request, CancellationToken ct)
    {
        var allFeatures = await _tenantRepository.GetAllFeaturesAsync(ct);

        // Not purchasable - IsImplemented:false features have no working code behind them (see
        // Feature.IsImplemented's own comment). A prerequisite that happens to be one of these
        // (e.g. session-management -> schedule-management, which is IsImplemented:false) is
        // silently dropped from the graph below rather than surfaced - that's a pre-existing gap
        // in FeatureDependencies.cs's own data, not something this bundle-builder feature should
        // paper over by refusing to sell session-management at all.
        var purchasable = allFeatures.Where(f => f.IsImplemented).ToList();
        var purchasableNames = purchasable.Select(f => f.Name).ToHashSet();

        var response = purchasable
            .Select(f => new BundleFeatureDto
            {
                Id = f.Id,
                Name = f.Name,
                DisplayName = f.DisplayName,
                Description = f.Description,
                Category = FeatureCategories.For(f.Name),
                BundlePrice = f.BundlePrice,
                IsCore = f.IsBundleCore,
                DirectPrerequisites = FeatureDependencies.GetPrerequisites(f.Name)
                    .Where(purchasableNames.Contains)
                    .ToList(),
                DirectDependents = FeatureDependencies.GetDependents(f.Name)
                    .Where(purchasableNames.Contains)
                    .ToList(),
            })
            .OrderBy(f => f.Category)
            .ThenBy(f => f.DisplayName)
            .ToList();

        return Result<List<BundleFeatureDto>>.Success(response, _operation);
    }
}
