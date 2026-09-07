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
}
