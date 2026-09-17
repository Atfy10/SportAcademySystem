using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Contract;

namespace SportAcademy.Application.Common.Regional;

public sealed class PhoneNumberNormalizer : IPhoneNumberNormalizer
{
    private readonly IRegionalValidationService _regionalValidation;
    private readonly ITenantSettingsCountryReader _countryReader;

    public PhoneNumberNormalizer(
        IRegionalValidationService regionalValidation,
        ITenantSettingsCountryReader countryReader)
    {
        _regionalValidation = regionalValidation;
        _countryReader = countryReader;
    }

    public async Task<string?> NormalizeAsync(string? phone, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(phone))
            return phone;

        var country = await _countryReader.GetCountryAsync(cancellationToken);
        return _regionalValidation.FormatPhoneE164(phone, country) ?? phone;
    }
}
