namespace SportAcademy.Application.DTOs.LimitDtos;

// One row per LimitedResources key, always - even a resource with no PlanLimit row for this
// plan is represented here with MaxCount null, so the console's Limits tab always renders a
// complete, stable set of controls rather than growing/shrinking rows as limits are added.
public record PlanLimitResponse
{
    public string ResourceKey { get; init; } = default!;

    /// <summary>Null = unlimited.</summary>
    public int? MaxCount { get; init; }
}
