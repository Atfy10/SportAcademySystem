using System.Collections.Frozen;
using System.Reflection;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using SportAcademy.Application.Common.Localization;
using SportAcademy.Domain.Contract;
using SportAcademy.Infrastructure.Services;

namespace SportAcademy.Infrastructure.Localization;

/// <summary>
/// Flat-JSON message catalogs, one file per language, loaded once at startup.
/// </summary>
/// <remarks>
/// JSON rather than .resx: the catalogs mirror the frontend's i18next files key-for-key, so one
/// translator glossary covers both sides, and there is no XML merge pain on a repo this active.
/// </remarks>
public sealed class JsonLocalizationService : ILocalizationService
{
    private readonly FrozenDictionary<string, FrozenDictionary<string, string>> _catalogs;
    private readonly ICurrentLanguageProvider _language;
    private readonly ILogger<JsonLocalizationService> _logger;

    public JsonLocalizationService(
        ICurrentLanguageProvider language,
        ILogger<JsonLocalizationService> logger)
    {
        _language = language;
        _logger = logger;
        _catalogs = Catalogs.Value;
    }

    private static readonly Lazy<FrozenDictionary<string, FrozenDictionary<string, string>>> Catalogs =
        new(Load, LazyThreadSafetyMode.ExecutionAndPublication);

    private static FrozenDictionary<string, FrozenDictionary<string, string>> Load()
    {
        var dir = Path.Combine(
            Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? AppContext.BaseDirectory,
            "Localization",
            "Resources");

        var loaded = new Dictionary<string, FrozenDictionary<string, string>>(StringComparer.OrdinalIgnoreCase);

        foreach (var lang in CurrentLanguageProvider.Supported)
        {
            var path = Path.Combine(dir, $"{lang}.json");
            if (!File.Exists(path))
            {
                loaded[lang] = FrozenDictionary<string, string>.Empty;
                continue;
            }

            var json = File.ReadAllText(path);
            var parsed = JsonSerializer.Deserialize<Dictionary<string, string>>(json)
                         ?? new Dictionary<string, string>();

            loaded[lang] = parsed.ToFrozenDictionary(StringComparer.Ordinal);
        }

        return loaded.ToFrozenDictionary(StringComparer.OrdinalIgnoreCase);
    }

    public string this[string key, params object[] args] => GetIn(_language.Language, key, args);

    public string GetIn(string language, string key, params object[] args)
    {
        if (string.IsNullOrWhiteSpace(key)) return string.Empty;

        var template = Lookup(CurrentLanguageProvider.Normalize(language), key)
                       ?? Lookup(CurrentLanguageProvider.Default, key);

        if (template is null)
        {
            // Never render a raw key to a user; callers treat this as "no translation available"
            // and keep whatever literal message they already had.
            _logger.LogDebug("Missing localization key {Key}", key);
            return key;
        }

        if (args is null || args.Length == 0) return template;

        try
        {
            return string.Format(template, args);
        }
        catch (FormatException)
        {
            // A malformed placeholder must not take down a request.
            _logger.LogWarning("Localization key {Key} has a malformed format template", key);
            return template;
        }
    }

    public bool Exists(string key) =>
        !string.IsNullOrWhiteSpace(key) &&
        (Lookup(_language.Language, key) is not null || Lookup(CurrentLanguageProvider.Default, key) is not null);

    private string? Lookup(string language, string key) =>
        _catalogs.TryGetValue(language, out var catalog) && catalog.TryGetValue(key, out var value)
            ? value
            : null;
}
