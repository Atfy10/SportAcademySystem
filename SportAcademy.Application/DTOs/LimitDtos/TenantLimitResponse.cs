namespace SportAcademy.Application.DTOs.LimitDtos;

// One row per LimitedResources key - the console's per-tenant usage-meter view (§5.2 of
// PLAN_LIMITS_DESIGN.md) and the wizard both read this shape.
public record TenantLimitResponse
{
    public string ResourceKey { get; init; } = default!;

    /// <summary>Null = unlimited.</summary>
    public int? MaxCount { get; init; }
    public int Used { get; init; }

    /// <summary>"Plan", "Override", or "Unlimited" - which of the two the number came from, so
    /// the console can render a "Custom" badge next to an overridden value.</summary>
    public string Source { get; init; } = default!;

    /// <summary>Only set when Source is "Override".</summary>
    public string? OverrideReason { get; init; }

    public bool IsOverLimit { get; init; }
}
