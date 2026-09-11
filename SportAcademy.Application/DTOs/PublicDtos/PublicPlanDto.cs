namespace SportAcademy.Application.DTOs.PublicDtos;

// What the public marketing site's pricing page renders. Intentionally leaner than the
// platform console's SubscriptionPlanSummaryDto - no Code (internal identifier, not a
// visitor's concern) and includes the plan's granted feature display names for the
// comparison table.
public record PublicPlanDto
{
    public int Id { get; init; }
    public string Name { get; init; } = default!;
    public string? Description { get; init; }
    public decimal MonthlyPrice { get; init; }
    public decimal YearlyPrice { get; init; }
    public bool IsHighlighted { get; init; }
    public int DisplayOrder { get; init; }
    public List<string> Features { get; init; } = [];
}
