using Microsoft.EntityFrameworkCore;
using SportAcademy.Application.Common.Regional;
using SportAcademy.Domain.Contract;
using SportAcademy.Infrastructure.Persistence.DBContext;

namespace SportAcademy.Infrastructure.Services;

public class TenantSettingsCountryReader : ITenantSettingsCountryReader
{
    private readonly ApplicationDbContext _context;
    private readonly ITenantIdProvider _tenantIdProvider;

    public TenantSettingsCountryReader(ApplicationDbContext context, ITenantIdProvider tenantIdProvider)
    {
        _context = context;
        _tenantIdProvider = tenantIdProvider;
    }

    public async Task<string> GetCountryAsync(CancellationToken cancellationToken = default)
    {
        var tenantId = _tenantIdProvider.TenantId;
        if (tenantId is null)
            return CountryRegionalRegistry.DefaultCountry;

        var country = await _context.TenantSettings
            .AsNoTracking()
            .Where(s => s.TenantId == tenantId)
            .Select(s => s.Country)
            .FirstOrDefaultAsync(cancellationToken);

        return country ?? CountryRegionalRegistry.DefaultCountry;
    }
}
