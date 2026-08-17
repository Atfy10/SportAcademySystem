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
}
