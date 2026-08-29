using System.Text;

namespace SportAcademy.Domain.Helpers;

/// <summary>
/// Folds an Arabic (or mixed Arabic/English) string down to a form that matches regardless of
/// spelling variation a user didn't intend as meaningful - alef-hamza forms, taa marbuta vs haa,
/// alef maqsura vs yaa, tatweel, harakat (diacritics), and Arabic-Indic digits.
/// </summary>
/// <remarks>
/// Without this, <c>.Contains()</c>/<c>LIKE</c> search is strict-byte matching over Unicode
/// Arabic, so "احمد" and "أحمد" - the same name, spelled two common ways - do not match each
/// other, and "ﻣﺤﻤﺪ" written with a decorative tatweel never matches "محمد" at all. English text
/// passes through unchanged (only characters in the Arabic ranges are touched), so this is safe
/// to apply to mixed-language input and columns.
/// </remarks>
public static class ArabicTextNormalizer
{
    // Alef variants -> bare alef (ا): hamza-above (أ), hamza-below (إ), madda (آ), wasla (ٱ).
    private const string AlefVariants = "أإآٱ";
    private const char Alef = 'ا';

    // Taa marbuta (ة) -> haa (ه) - interchangeable in casual writing and search intent.
    private const char TaaMarbuta = 'ة';
    private const char Haa = 'ه';

    // Alef maqsura (ى) -> yaa (ي) - same reasoning.
    private const char AlefMaqsura = 'ى';
    private const char Yaa = 'ي';

    // Tatweel/kashida: a purely decorative elongation character with no meaning.
    private const char Tatweel = '\u0640';

    // Base Arabic letters occupy U+0621-U+064A - the diacritic ranges below must stop before
    // that block, not span across it, or real letters (e.g. seen at U+0633) get silently
    // stripped as if they were diacritics.
    // U+0610-U+061A: Quranic honorific marks. U+064B-U+0652: the core harakat (fathatan,
    // dammatan, kasratan, fatha, damma, kasra, shadda, sukoon). U+0670: superscript alef.
    // U+06D6-U+06ED: Quranic annotation signs.
    private static bool IsArabicDiacritic(char c) =>
        (c >= '\u0610' && c <= '\u061A') ||
        (c >= '\u064B' && c <= '\u0652') ||
        c == '\u0670' ||
        (c >= '\u06D6' && c <= '\u06ED');

    // Arabic-Indic (٠-٩, U+0660-0669) and Extended Arabic-Indic (۰-۹, U+06F0-06F9) digits -> ASCII.
    private static char FoldDigit(char c)
    {
        if (c is >= '\u0660' and <= '\u0669') return (char)('0' + (c - '\u0660'));
        if (c is >= '\u06F0' and <= '\u06F9') return (char)('0' + (c - '\u06F0'));
        return c;
    }

    /// <summary>
    /// Normalizes for search comparison: folds letter variants, strips tatweel and diacritics,
    /// folds digits to ASCII, trims/collapses whitespace, and uppercase-invariant folds any
    /// Latin text so the same function handles mixed-language input in one pass.
    /// </summary>
    public static string Normalize(string? value)
    {
        if (string.IsNullOrEmpty(value)) return string.Empty;

        var builder = new StringBuilder(value.Length);
        var lastWasSpace = false;

        foreach (var raw in value)
        {
            if (IsArabicDiacritic(raw) || raw == Tatweel) continue;

            var c = raw switch
            {
                _ when AlefVariants.IndexOf(raw) >= 0 => Alef,
                TaaMarbuta => Haa,
                AlefMaqsura => Yaa,
                _ => FoldDigit(raw),
            };

            if (char.IsWhiteSpace(c))
            {
                if (lastWasSpace) continue;
                lastWasSpace = true;
                builder.Append(' ');
                continue;
            }

            lastWasSpace = false;
            builder.Append(char.ToUpperInvariant(c));
        }

        return builder.ToString().Trim();
    }

    /// <summary>True when the normalized forms of both strings are equal.</summary>
    public static bool NormalizedEquals(string? a, string? b) =>
        string.Equals(Normalize(a), Normalize(b), StringComparison.Ordinal);
}
