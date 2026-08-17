namespace SportAcademy.Application.DTOs.TenantDtos;

public record TenantFeaturesListDto
{
    public List<TenantFeatureDto> Features { get; init; } = [];
    public Dictionary<string, List<TenantFeatureDto>> ByCategory { get; init; } = [];
}
