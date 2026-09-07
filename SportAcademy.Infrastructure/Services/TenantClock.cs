using Microsoft.EntityFrameworkCore;
using SportAcademy.Domain.Contract;
using SportAcademy.Infrastructure.Persistence.DBContext;

namespace SportAcademy.Infrastructure.Services;

public class TenantClock : ITenantClock
{
    private readonly ApplicationDbContext _context;
    private readonly ITenantIdProvider _tenantIdProvider;

    public TenantClock(ApplicationDbContext context, ITenantIdProvider tenantIdProvider)
    {
        _context = context;
        _tenantIdProvider = tenantIdProvider;
    }

    public async Task<DateTime> GetLocalNowAsync(CancellationToken cancellationToken = default)
    {
        var utcNow = DateTime.UtcNow;
        var tenantId = _tenantIdProvider.TenantId;
        if (tenantId is null) return utcNow;

        var timeZoneId = await _context.TenantSettings
            .AsNoTracking()
            .Where(s => s.TenantId == tenantId)
            .Select(s => s.TimeZone)
            .FirstOrDefaultAsync(cancellationToken);

        if (string.IsNullOrWhiteSpace(timeZoneId)) return utcNow;

        try
        {
            var timeZone = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
            return TimeZoneInfo.ConvertTimeFromUtc(utcNow, timeZone);
        }
        catch (Exception ex) when (ex is TimeZoneNotFoundException or InvalidTimeZoneException)
        {
            // An invalid/unrecognized IANA id in TenantSettings.TimeZone must not break session
            // generation or attendance marking - fall back to UTC rather than throwing.
            return utcNow;
        }
    }
}
