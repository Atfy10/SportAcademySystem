namespace SportAcademy.Application.DTOs.TenantDtos;

public record TenantSettingsDto
{
    public string TimeZone { get; init; } = default!;
    public string Language { get; init; } = default!;
    public string DateFormat { get; init; } = default!;
    public string TimeFormat { get; init; } = default!;
    public string Currency { get; init; } = default!;
}
