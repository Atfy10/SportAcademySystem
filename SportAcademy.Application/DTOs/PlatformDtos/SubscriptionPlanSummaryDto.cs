namespace SportAcademy.Application.DTOs.PlatformDtos;

// The real subscription-plan catalog, for the Platform console's plan pickers (tenant creation,
// change-plan) to build from instead of a hardcoded {1,2,3} = {Basic,Pro,Enterprise} guess that
// silently drifts from whatever rows actually exist in SubscriptionPlans.
public record SubscriptionPlanSummaryDto
{
    public int Id { get; init; }
    public string Name { get; init; } = default!;
    public string Code { get; init; } = default!;
    public decimal MonthlyPrice { get; init; }
    public decimal YearlyPrice { get; init; }
}
