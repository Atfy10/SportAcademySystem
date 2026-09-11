using MediatR;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.DTOs.PlatformDtos;
using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Entities.Tenants;
using SportAcademy.Domain.Enums;

namespace SportAcademy.Application.Queries.PlatformQueries.GetSubscriptionPlans;

public class GetSubscriptionPlansQueryHandler
    : IRequestHandler<GetSubscriptionPlansQuery, Result<List<SubscriptionPlanSummaryDto>>>
{
    private readonly IBaseRepository<SubscriptionPlan, int> _planRepository;
    private readonly string _operation = OperationType.Get.ToString();

    public GetSubscriptionPlansQueryHandler(IBaseRepository<SubscriptionPlan, int> planRepository)
    {
        _planRepository = planRepository;
    }

    public async Task<Result<List<SubscriptionPlanSummaryDto>>> Handle(
        GetSubscriptionPlansQuery request, CancellationToken ct)
    {
        var plans = await _planRepository.GetAllAsync(ct);

        var response = plans
            .Where(p => p.IsActive)
            .OrderBy(p => p.MonthlyPrice)
            .Select(p => new SubscriptionPlanSummaryDto
            {
                Id = p.Id,
                Name = p.Name,
                Code = p.Code,
                Description = p.Description,
                MonthlyPrice = p.MonthlyPrice,
                YearlyPrice = p.YearlyPrice,
                IsPubliclyListed = p.IsPubliclyListed,
                DisplayOrder = p.DisplayOrder,
                IsHighlighted = p.IsHighlighted,
            })
            .ToList();

        return Result<List<SubscriptionPlanSummaryDto>>.Success(response, _operation);
    }
}
