namespace SportAcademy.Application.Common.Regional;

/// <summary>Display + validation metadata for one country in the Settings country picker and
/// the shared regional-validation service. Phone dial codes/formats are NOT duplicated here -
/// PhoneNumbers (libphonenumber) already owns that; DialCode below is display-only, for the
/// Settings dropdown and pre-filling a phone input.</summary>
public sealed record CountryRegionalProfile(
    string IsoCode,
    string Name,
    string DialCode,
    NationalIdRule NationalIdRule);
