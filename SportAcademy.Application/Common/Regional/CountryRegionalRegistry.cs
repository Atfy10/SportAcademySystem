using System.Text.RegularExpressions;
using SportAcademy.Domain.Helpers;

namespace SportAcademy.Application.Common.Regional;

/// <summary>
/// Single source of truth for per-country National ID rules, plus the display metadata the
/// Settings country picker needs. Adding a country later is one new entry here - nothing else
/// in the codebase needs to change (every validator/schema goes through this registry, not a
/// hardcoded rule of its own). Phone number validation does NOT live here: it goes through
/// PhoneNumbers (libphonenumber, see RegionalValidationService), which already knows every
/// country's dial code and number format.
/// </summary>
public static class CountryRegionalRegistry
{
    public const string DefaultCountry = "KW";

    /// <summary>Kuwait's exact checksum, unchanged - delegates to the pre-existing helper so
    /// current Kuwait-tenant validation behavior is byte-identical to before this feature.</summary>
    private static bool ValidateKuwait(string ssn, DateOnly? birthDate)
        => birthDate is { } dob && PersonValidationHelper.IsValidSSN(ssn, dob);

    /// <summary>
    /// Egypt's national ID structure: digit 1 = century marker (2=1900s, 3=2000s), digits 2-7 =
    /// YYMMDD birth date, digits 8-9 = governorate code, digits 10-13 = sequence, digit 14 =
    /// checksum. Length/digits-only/embedded-date are enforced here; the checksum digit's exact
    /// formula is NOT verified against an authoritative source and is accepted as-is (any
    /// digit) - confirm the real algorithm before relying on this for uniqueness enforcement.
    /// </summary>
    private static bool ValidateEgypt(string nationalId, DateOnly? _)
    {
        var century = nationalId[0] switch { '2' => 1900, '3' => 2000, _ => (int?)null };
        if (century is null)
            return false;

        var yy = int.Parse(nationalId.Substring(1, 2));
        var month = int.Parse(nationalId.Substring(3, 2));
        var day = int.Parse(nationalId.Substring(5, 2));
        if (month is < 1 or > 12)
            return false;

        var daysInMonth = DateTime.DaysInMonth(century.Value + yy, month);
        return day >= 1 && day <= daysInMonth;
    }

    public static readonly IReadOnlyDictionary<string, CountryRegionalProfile> Countries =
        new Dictionary<string, CountryRegionalProfile>(StringComparer.OrdinalIgnoreCase)
        {
            ["KW"] = new("KW", "Kuwait", "+965", new NationalIdRule
            {
                FixedLength = 12,
                Pattern = new Regex(@"^\d{12}$", RegexOptions.Compiled),
                ChecksumValidator = ValidateKuwait,
            }),
            ["EG"] = new("EG", "Egypt", "+20", new NationalIdRule
            {
                FixedLength = 14,
                Pattern = new Regex(@"^\d{14}$", RegexOptions.Compiled),
                ChecksumValidator = ValidateEgypt,
            }),
            // Saudi national ID (citizens) / Iqama (residents): 10 digits. No checksum formula
            // encoded yet - confirm before relying on this for uniqueness enforcement.
            ["SA"] = new("SA", "Saudi Arabia", "+966", new NationalIdRule
            {
                FixedLength = 10,
                Pattern = new Regex(@"^\d{10}$", RegexOptions.Compiled),
            }),
            // UAE Emirates ID, canonical digits-only form (without the 784-YYYY-NNNNNNN-C
            // display dashes/checksum digit): 15 digits.
            ["AE"] = new("AE", "United Arab Emirates", "+971", new NationalIdRule
            {
                FixedLength = 15,
                Pattern = new Regex(@"^\d{15}$", RegexOptions.Compiled),
            }),
            // Bahrain CPR number: 9 digits.
            ["BH"] = new("BH", "Bahrain", "+973", new NationalIdRule
            {
                FixedLength = 9,
                Pattern = new Regex(@"^\d{9}$", RegexOptions.Compiled),
            }),
            // Qatar QID: 11 digits.
            ["QA"] = new("QA", "Qatar", "+974", new NationalIdRule
            {
                FixedLength = 11,
                Pattern = new Regex(@"^\d{11}$", RegexOptions.Compiled),
            }),
            // Oman civil number: 8 digits.
            ["OM"] = new("OM", "Oman", "+968", new NationalIdRule
            {
                FixedLength = 8,
                Pattern = new Regex(@"^\d{8}$", RegexOptions.Compiled),
            }),
        };

    /// <summary>Fallback for any country not explicitly listed above: digits only, a broad
    /// reasonable length, never required.</summary>
    public static readonly NationalIdRule GenericFallbackRule = new()
    {
        FixedLength = null,
        Pattern = new Regex(@"^\d{5,20}$", RegexOptions.Compiled),
    };

    public static NationalIdRule GetNationalIdRule(string? countryIso)
        => countryIso is not null && Countries.TryGetValue(countryIso, out var profile)
            ? profile.NationalIdRule
            : GenericFallbackRule;
}
