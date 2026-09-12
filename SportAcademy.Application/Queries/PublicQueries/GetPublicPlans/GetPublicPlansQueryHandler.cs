using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.DTOs.PublicDtos;
using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Contract;
using SportAcademy.Domain.Entities.Tenants;
using SportAcademy.Domain.Enums;

namespace SportAcademy.Application.Queries.PublicQueries.GetPublicPlans;

public class GetPublicPlansQueryHandler : IRequestHandler<GetPublicPlansQuery, Result<List<PublicPlanDto>>>
{
    private readonly IBaseRepository<SubscriptionPlan, int> _planRepository;
    private readonly ITenantRepository _tenantRepository;
    private readonly string _operation = OperationType.Get.ToString();

    public GetPublicPlansQueryHandler(
        IBaseRepository<SubscriptionPlan, int> planRepository, ITenantRepository tenantRepository)
    {
        _planRepository = planRepository;
        _tenantRepository = tenantRepository;
    }

    public async Task<Result<List<PublicPlanDto>>> Handle(GetPublicPlansQuery request, CancellationToken ct)
    {
        var plans = await _planRepository.GetAllAsync(ct);
        var visiblePlans = plans
            .Where(p => p.IsActive && p.IsPubliclyListed)
            .OrderBy(p => p.DisplayOrder)
            .ThenBy(p => p.MonthlyPrice)
            .ToList();

        var allFeatures = await _tenantRepository.GetAllFeaturesAsync(ct);
        var featureNameById = allFeatures.ToDictionary(f => f.Id, f => f.DisplayName);

        var response = new List<PublicPlanDto>();
        foreach (var plan in visiblePlans)
        {
            var featureIds = await _tenantRepository.GetPlanFeaturesAsync(plan.Id, ct);
            response.Add(new PublicPlanDto
            {
                Id = plan.Id,
                Name = plan.Name,
                Description = plan.Description,
                MonthlyPrice = plan.MonthlyPrice,
                YearlyPrice = plan.YearlyPrice,
                IsHighlighted = plan.IsHighlighted,
                DisplayOrder = plan.DisplayOrder,
                Features = featureIds
                    .Where(featureNameById.ContainsKey)
                    .Select(id => featureNameById[id])
                    .OrderBy(name => name)
                    .ToList(),
            });
        }

        return Result<List<PublicPlanDto>>.Success(response, _operation);
    }
}
