using PhoneNumbers;
using SportAcademy.Application.Interfaces;

namespace SportAcademy.Application.Common.Regional;

/// <summary>Pure logic, no I/O - safe as a singleton. PhoneNumberUtil.GetInstance() is itself a
/// cached singleton internally, so constructing this cheaply is fine per-call too.</summary>
public sealed class RegionalValidationService : IRegionalValidationService
{
    private readonly PhoneNumberUtil _phoneUtil = PhoneNumberUtil.GetInstance();

    public bool IsValidPhoneNumber(string? phone, string countryIso)
    {
        if (string.IsNullOrWhiteSpace(phone))
            return false;

        try
        {
            var parsed = _phoneUtil.Parse(phone, countryIso);
            return _phoneUtil.IsValidNumber(parsed);
        }
        catch (NumberParseException)
        {
            return false;
        }
    }

    public string? FormatPhoneE164(string? phone, string countryIso)
    {
        if (string.IsNullOrWhiteSpace(phone))
            return null;

        try
        {
            var parsed = _phoneUtil.Parse(phone, countryIso);
            return _phoneUtil.IsValidNumber(parsed)
                ? _phoneUtil.Format(parsed, PhoneNumberFormat.E164)
                : null;
        }
        catch (NumberParseException)
        {
            return null;
        }
    }

    public bool IsValidNationalId(string? nationalId, string countryIso, DateOnly? birthDate)
        => CountryRegionalRegistry.GetNationalIdRule(countryIso).IsValid(nationalId, birthDate);
}
