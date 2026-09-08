namespace SportAcademy.Application.DTOs.TenantDtos;

public record TenantFeatureDto
{
    public Guid FeatureId { get; init; }
    public string Name { get; init; } = default!;
    public string DisplayName { get; init; } = default!;
    public string? Description { get; init; }
    public string Category { get; init; } = default!;
    public bool IsEnabled { get; init; }
    public bool CanToggle { get; init; }
    public bool LockedBySuperAdmin { get; init; }
    public DateTime? EnabledAt { get; init; }
    // Direct "requires" edges from FeatureDependencies - not transitive, matches what the UI
    // shows as a "Requires: X, Y" badge next to the toggle.
    public IReadOnlyList<string> DependsOn { get; init; } = [];
    // Direct reverse edges - features that name this one as a prerequisite, regardless of their
    // own current enabled state (shown as "Required by: X, Y").
    public IReadOnlyList<string> RequiredBy { get; init; } = [];
    // FeatureDependencies.IsProtected - a foundational capability the tenant can never disable
    // themselves (CanToggle is already false here for this reason); a SuperAdmin can, from the
    // Platform console, with explicit confirmation. Render like a locked-on feature: switch
    // disabled here, badge explaining why.
    public bool IsProtected { get; init; }
    // Feature.IsImplemented - false means this catalog entry has no code behind it yet (see
    // AppDataSeeder.FeatureCatalog's own comment for the current list and why). CanToggle is
    // already false here for this reason too - nothing to toggle - and the UI shows a
    // "not available yet" badge instead of the usual enabled/disabled one.
    public bool IsImplemented { get; init; }
}
