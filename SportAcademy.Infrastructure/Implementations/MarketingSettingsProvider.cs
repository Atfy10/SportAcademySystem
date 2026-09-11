using Microsoft.Extensions.Options;
using SportAcademy.Domain.Contract;
using SportAcademy.Infrastructure.Options;

namespace SportAcademy.Infrastructure.Implementations;

public sealed class MarketingSettingsProvider : IMarketingSettingsProvider
{
    private readonly MarketingSettings _settings;

    public MarketingSettingsProvider(IOptions<MarketingSettings> settings)
    {
        _settings = settings.Value;
    }

    public string SalesInboxEmail => _settings.SalesInboxEmail;
}
