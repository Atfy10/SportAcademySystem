namespace SportAcademy.Application.DTOs.TenantDtos;

public record TenantSettingsOptionsDto
{
    public List<string> Timezones { get; init; } = [];
    public List<LanguageOption> Languages { get; init; } = [];
    public List<CurrencyOption> Currencies { get; init; } = [];
    public List<string> DateFormats { get; init; } = [];
    public List<string> TimeFormats { get; init; } = [];
    public List<CountryOption> Countries { get; init; } = [];
}

public record LanguageOption(string Code, string Name);

public record CurrencyOption(string Code, string Symbol);

/// <summary>One country's dial code + National ID shape, enough for the frontend to render a
/// country picker and build a matching National ID zod schema without hardcoding its own copy
/// of the rule (single source of truth stays in CountryRegionalRegistry).</summary>
public record CountryOption(
    string IsoCode,
    string Name,
    string DialCode,
    int? NationalIdLength,
    string NationalIdPattern);
