using MediatR;
using SportAcademy.Application.Common.Limits;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.DTOs.LimitDtos;
using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Contract;
using SportAcademy.Domain.Entities.Tenants;
using SportAcademy.Domain.Enums;

namespace SportAcademy.Application.Queries.PlatformQueries.GetPlanLimits;

public class GetPlanLimitsQueryHandler : IRequestHandler<GetPlanLimitsQuery, Result<List<PlanLimitResponse>>>
{
    private readonly IBaseRepository<SubscriptionPlan, int> _planRepository;
    private readonly ITenantRepository _tenantRepository;
    private readonly string _operation = OperationType.Get.ToString();

    public GetPlanLimitsQueryHandler(IBaseRepository<SubscriptionPlan, int> planRepository, ITenantRepository tenantRepository)
    {
        _planRepository = planRepository;
        _tenantRepository = tenantRepository;
    }

    public async Task<Result<List<PlanLimitResponse>>> Handle(GetPlanLimitsQuery request, CancellationToken ct)
    {
        var plan = await _planRepository.GetByIdAsync(request.PlanId, ct);
        if (plan is null)
            return Result<List<PlanLimitResponse>>.Failure(_operation, "Subscription plan not found.", 404);

        var existing = (await _tenantRepository.GetPlanLimitsAsync(request.PlanId, ct))
            .ToDictionary(l => l.ResourceKey, l => l.MaxCount);

        // Every known resource is represented, even one with no row yet (implicitly unlimited) -
        // see PlanLimitResponse's own comment.
        var response = LimitedResources.All
            .Select(key => new PlanLimitResponse
            {
                ResourceKey = key,
                MaxCount = existing.GetValueOrDefault(key),
            })
            .ToList();

        return Result<List<PlanLimitResponse>>.Success(response, _operation);
    }
}
