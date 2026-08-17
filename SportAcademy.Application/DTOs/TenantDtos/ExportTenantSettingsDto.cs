namespace SportAcademy.Application.DTOs.TenantDtos;

public record ExportTenantSettingsDto
{
    public int Version { get; init; } = 1;
    public DateTime ExportedAt { get; init; }
    public TenantSettingsDto Settings { get; init; } = default!;
    public List<ExportTenantFeatureDto> Features { get; init; } = [];
}

public record ExportTenantFeatureDto
{
    public Guid FeatureId { get; init; }
    public string Name { get; init; } = default!;
    public bool IsEnabled { get; init; }
    public DateTime? EnabledAt { get; init; }
}
