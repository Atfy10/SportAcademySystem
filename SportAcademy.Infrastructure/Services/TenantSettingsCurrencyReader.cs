using Microsoft.EntityFrameworkCore;
using SportAcademy.Domain.Contract;
using SportAcademy.Infrastructure.Persistence.DBContext;

namespace SportAcademy.Infrastructure.Services;

public class TenantSettingsCurrencyReader : ITenantSettingsCurrencyReader
{
    private readonly ApplicationDbContext _context;
    private readonly ITenantIdProvider _tenantIdProvider;

    public TenantSettingsCurrencyReader(ApplicationDbContext context, ITenantIdProvider tenantIdProvider)
    {
        _context = context;
        _tenantIdProvider = tenantIdProvider;
    }

    public async Task<string?> GetCurrencyAsync(CancellationToken cancellationToken = default)
    {
        var tenantId = _tenantIdProvider.TenantId;
        if (tenantId is null) return null;

        return await _context.TenantSettings
            .AsNoTracking()
            .Where(s => s.TenantId == tenantId)
            .Select(s => s.Currency)
            .FirstOrDefaultAsync(cancellationToken);
    }
}
