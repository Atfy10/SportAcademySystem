namespace SportAcademy.Application.DTOs.PlatformDtos;

public record TenantFeatureResponse
{
    public Guid FeatureId { get; init; }
    public string Name { get; init; } = default!;
    public string DisplayName { get; init; } = default!;
    public string? Description { get; init; }
    public bool IsEnabled { get; init; }
    public DateTime? EnabledAt { get; init; }
}
