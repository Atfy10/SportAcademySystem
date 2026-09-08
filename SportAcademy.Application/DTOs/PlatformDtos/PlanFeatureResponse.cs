namespace SportAcademy.Application.DTOs.PlatformDtos;

// A plan's own feature membership - distinct from TenantFeatureResponse, which is a tenant's
// per-feature runtime state (IsEnabled/LockedBySuperAdmin/EnabledBy). IsIncluded here just means
// "this plan grants this feature," nothing about whether any tenant has actually turned it on.
public record PlanFeatureResponse
{
    public Guid FeatureId { get; init; }
    public string Name { get; init; } = default!;
    public string DisplayName { get; init; } = default!;
    public string? Description { get; init; }
    public string Category { get; init; } = default!;
    public bool IsIncluded { get; init; }
}
