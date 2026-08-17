namespace SportAcademy.Application.DTOs.TenantDtos;

public record TenantSettingsOptionsDto
{
    public List<string> Timezones { get; init; } = [];
    public List<LanguageOption> Languages { get; init; } = [];
    public List<CurrencyOption> Currencies { get; init; } = [];
    public List<string> DateFormats { get; init; } = [];
    public List<string> TimeFormats { get; init; } = [];
}

public record LanguageOption(string Code, string Name);

public record CurrencyOption(string Code, string Symbol);
