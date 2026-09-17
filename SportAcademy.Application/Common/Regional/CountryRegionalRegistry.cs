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
    /// Shared by every launch country whose National ID embeds the holder's birth date as a
    /// 7-digit prefix (Kuwait's own format, kept separately above for byte-identical behavior,
    /// follows this same rule): digit 1 = century marker ('2' for a birth year &lt;= 1999, '3'
    /// for &gt;= 2000), digits 2-7 = the birth date as YYMMDD. This cross-checks the prefix
    /// against the person's actual BirthDate field - it is not just a "is this a plausible
    /// calendar date" structural check, the two must genuinely match.
    /// </summary>
    private static bool ValidateBirthDatePrefix(string nationalId, DateOnly? birthDate)
    {
        if (birthDate is not { } dob)
            return false;

        var expectedPrefix = (dob.Year > 1999 ? "3" : "2") + dob.ToString("yyMMdd");
        return nationalId.StartsWith(expectedPrefix, StringComparison.Ordinal);
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
            // Egypt: 14 digits - digits 8-9 governorate code, digits 10-13 sequence, digit 14 a
            // checksum digit whose exact formula is NOT verified against an authoritative source
            // (accepted as-is) - only the digit 1-7 birth-date prefix is cross-checked.
            ["EG"] = new("EG", "Egypt", "+20", new NationalIdRule
            {
                FixedLength = 14,
                Pattern = new Regex(@"^\d{14}$", RegexOptions.Compiled),
                ChecksumValidator = ValidateBirthDatePrefix,
            }),
            // Saudi national ID (citizens) / Iqama (residents): 10 digits, digits 8-10 a serial
            // + Luhn check digit not validated here - only the digit 1-7 birth-date prefix is
            // cross-checked.
            ["SA"] = new("SA", "Saudi Arabia", "+966", new NationalIdRule
            {
                FixedLength = 10,
                Pattern = new Regex(@"^\d{10}$", RegexOptions.Compiled),
                ChecksumValidator = ValidateBirthDatePrefix,
            }),
            // UAE Emirates ID, canonical digits-only form (without the 784-YYYY-NNNNNNN-C
            // display dashes/checksum digit): 15 digits - only the digit 1-7 birth-date prefix
            // is cross-checked, digits 8-15 (serial + checksum) are not.
            ["AE"] = new("AE", "United Arab Emirates", "+971", new NationalIdRule
            {
                FixedLength = 15,
                Pattern = new Regex(@"^\d{15}$", RegexOptions.Compiled),
                ChecksumValidator = ValidateBirthDatePrefix,
            }),
            // Bahrain CPR number: 9 digits - only the digit 1-7 birth-date prefix is
            // cross-checked, digits 8-9 (serial) are not.
            ["BH"] = new("BH", "Bahrain", "+973", new NationalIdRule
            {
                FixedLength = 9,
                Pattern = new Regex(@"^\d{9}$", RegexOptions.Compiled),
                ChecksumValidator = ValidateBirthDatePrefix,
            }),
            // Qatar QID: 11 digits - only the digit 1-7 birth-date prefix is cross-checked,
            // digits 8-11 (serial) are not.
            ["QA"] = new("QA", "Qatar", "+974", new NationalIdRule
            {
                FixedLength = 11,
                Pattern = new Regex(@"^\d{11}$", RegexOptions.Compiled),
                ChecksumValidator = ValidateBirthDatePrefix,
            }),
            // Oman civil number: 8 digits - only the digit 1-7 birth-date prefix is
            // cross-checked, digit 8 (serial) is not.
            ["OM"] = new("OM", "Oman", "+968", new NationalIdRule
            {
                FixedLength = 8,
                Pattern = new Regex(@"^\d{8}$", RegexOptions.Compiled),
                ChecksumValidator = ValidateBirthDatePrefix,
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
