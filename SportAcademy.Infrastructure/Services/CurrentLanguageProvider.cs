using System.Globalization;
using SportAcademy.Domain.Contract;

namespace SportAcademy.Infrastructure.Services;

public class CurrentLanguageProvider : ICurrentLanguageProvider
{
    /// <summary>Languages the application actually ships catalogs for.</summary>
    public static readonly string[] Supported = ["en", "ar"];

    public const string Default = "en";

    private static readonly AsyncLocal<string?> _language = new();

    public string Language => _language.Value ?? Default;

    public void SetLanguage(string? language) => _language.Value = Normalize(language);

    /// <summary>
    /// Reduces anything that might arrive as a language tag to a supported neutral code.
    /// TenantSettings stores a regional code ("ar-KW") while the catalogs are neutral ("ar"),
    /// and an Accept-Language header can carry anything at all.
    /// </summary>
    public static string Normalize(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return Default;

        var candidate = value.Trim();

        try
        {
            candidate = CultureInfo.GetCultureInfo(candidate).TwoLetterISOLanguageName;
        }
        catch (CultureNotFoundException)
        {
            // Not a culture name - fall back to splitting off any region suffix ourselves.
            candidate = candidate.Split('-', '_')[0];
        }

        return Supported.Contains(candidate, StringComparer.OrdinalIgnoreCase)
            ? candidate.ToLowerInvariant()
            : Default;
    }
}
