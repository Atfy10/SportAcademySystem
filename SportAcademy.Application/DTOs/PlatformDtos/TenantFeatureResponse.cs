namespace SportAcademy.Application.DTOs.PlatformDtos;

// Deliberately the same shape as TenantDtos.TenantFeatureDto (the Owner's Settings page) - the
// SuperAdmin's per-tenant Features tab must show the exact same category grouping and lock state
// for the exact same tenant, not an approximation reconstructed client-side. CanToggle has no
// equivalent here: a SuperAdmin can always force any feature either way regardless of the
// tenant's plan or current lock state (see ToggleFeatureCommandHandler) - LockedBySuperAdmin only
// tells the SuperAdmin whether their own past decision is what's currently holding this value.
public record TenantFeatureResponse
{
    public Guid FeatureId { get; init; }
    public string Name { get; init; } = default!;
    public string DisplayName { get; init; } = default!;
    public string? Description { get; init; }
    public string Category { get; init; } = default!;
    public bool IsEnabled { get; init; }
    public bool LockedBySuperAdmin { get; init; }
    public string? EnabledBy { get; init; }
    public DateTime? EnabledAt { get; init; }
    // Same DependsOn/RequiredBy fields as TenantDtos.TenantFeatureDto, for the same reason this
    // whole record mirrors it - the SuperAdmin's per-tenant Features tab shows the identical
    // dependency badges the Owner's own Settings page shows.
    public IReadOnlyList<string> DependsOn { get; init; } = [];
    public IReadOnlyList<string> RequiredBy { get; init; } = [];
    // Same IsProtected semantics as TenantDtos.TenantFeatureDto - here it means the Platform
    // console's own toggle must warn-and-confirm before disabling instead of firing immediately
    // (see ToggleFeatureCommand.ConfirmProtectedDisable), not that it's blocked outright.
    public bool IsProtected { get; init; }
    // Same IsImplemented semantics as TenantDtos.TenantFeatureDto - the Platform console's
    // switch is disabled for these too (see AppDataSeeder.FeatureCatalog): forcing a toggle a
    // SuperAdmin can always override has no reason to stay live for a feature nothing enforces.
    public bool IsImplemented { get; init; }
}
