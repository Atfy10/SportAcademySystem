using SportAcademy.Domain.Contract;

namespace SportAcademy.Tests.Application.Validators;

/// <summary>Test double for ITenantSettingsCountryReader - always resolves to a fixed country,
/// since validator unit tests run with no ambient tenant/DbContext.</summary>
public sealed class FixedCountryReader(string country) : ITenantSettingsCountryReader
{
    public Task<string> GetCountryAsync(CancellationToken cancellationToken = default)
        => Task.FromResult(country);
}
