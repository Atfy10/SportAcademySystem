using MediatR;
using SportAcademy.Application.Common.Features;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.DTOs.PlatformDtos;
using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Contract;
using SportAcademy.Domain.Entities.Tenants;
using SportAcademy.Domain.Enums;

namespace SportAcademy.Application.Queries.PlatformQueries.GetPlanFeatures;

public class GetPlanFeaturesQueryHandler : IRequestHandler<GetPlanFeaturesQuery, Result<List<PlanFeatureResponse>>>
{
    private readonly IBaseRepository<SubscriptionPlan, int> _planRepository;
    private readonly ITenantRepository _tenantRepository;
    private readonly string _operation = OperationType.Get.ToString();

    public GetPlanFeaturesQueryHandler(
        IBaseRepository<SubscriptionPlan, int> planRepository, ITenantRepository tenantRepository)
    {
        _planRepository = planRepository;
        _tenantRepository = tenantRepository;
    }

    public async Task<Result<List<PlanFeatureResponse>>> Handle(GetPlanFeaturesQuery request, CancellationToken ct)
    {
        var plan = await _planRepository.GetByIdAsync(request.PlanId, ct);
        if (plan is null)
            return Result<List<PlanFeatureResponse>>.Failure(_operation, "Subscription plan not found.", 404);

        var allFeatures = await _tenantRepository.GetAllFeaturesAsync(ct);
        var includedIds = (await _tenantRepository.GetPlanFeaturesAsync(request.PlanId, ct)).ToHashSet();

        var response = allFeatures.Select(f => new PlanFeatureResponse
        {
            FeatureId = f.Id,
            Name = f.Name,
            DisplayName = f.DisplayName,
            Description = f.Description,
            Category = FeatureCategories.For(f.Name),
            IsIncluded = includedIds.Contains(f.Id)
        }).ToList();

        return Result<List<PlanFeatureResponse>>.Success(response, _operation);
    }
}
