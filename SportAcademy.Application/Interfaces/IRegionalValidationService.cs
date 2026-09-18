namespace SportAcademy.Application.Interfaces;

/// <summary>
/// Country-aware phone number and National ID validation, replacing the per-validator
/// hardcoded Kuwait regexes that used to be duplicated across ~9 backend validators. Phone
/// rules come from libphonenumber (via the PhoneNumbers wrapper below); National ID rules come
/// from SportAcademy.Application.Common.Regional.CountryRegionalRegistry.
/// </summary>
public interface IRegionalValidationService
{
    bool IsValidPhoneNumber(string? phone, string countryIso);

    /// <summary>Normalizes a phone number to E.164 (e.g. "+96551234567") for storage. Returns
    /// null when the number doesn't parse - callers should validate first.</summary>
    string? FormatPhoneE164(string? phone, string countryIso);

    bool IsValidNationalId(string? nationalId, string countryIso, DateOnly? birthDate);
}
