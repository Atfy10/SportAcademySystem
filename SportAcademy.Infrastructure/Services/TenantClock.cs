using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using SportAcademy.Domain.Contract;
using SportAcademy.Domain.Services;
using SportAcademy.Infrastructure.Persistence.DBContext;

namespace SportAcademy.Infrastructure.Services;

public class TenantClock : ITenantClock
{
    private static readonly TimeSpan CacheFor = TimeSpan.FromMinutes(10);

    private readonly ApplicationDbContext _context;
    private readonly ITenantIdProvider _tenantIdProvider;
    private readonly IMemoryCache? _cache;

    public TenantClock(ApplicationDbContext context, ITenantIdProvider tenantIdProvider, IMemoryCache? cache = null)
    {
        _context = context;
        _tenantIdProvider = tenantIdProvider;
        _cache = cache;
    }

    public async Task<DateTime> GetLocalNowAsync(CancellationToken cancellationToken = default)
    {
        var utcNow = DateTime.UtcNow;
        var tenantId = _tenantIdProvider.TenantId;
        if (tenantId is null) return utcNow;

        // Cached per tenant: this now runs once per request (to set TenantCalendar.Today), and a
        // tenant's time zone changes about never. An invalid/unrecognized IANA id must not break
        // session generation or attendance marking - it falls back to UTC.
        var timeZoneId = await GetTimeZoneIdAsync(tenantId.Value, cancellationToken);
        var timeZone = TenantCalendar.FindTimeZone(timeZoneId);
        return timeZone is null ? utcNow : TimeZoneInfo.ConvertTimeFromUtc(utcNow, timeZone);
    }

    public async Task<TimeZoneInfo?> GetTimeZoneAsync(CancellationToken cancellationToken = default)
    {
        var tenantId = _tenantIdProvider.TenantId;
        return tenantId is null
            ? null
            : TenantCalendar.FindTimeZone(await GetTimeZoneIdAsync(tenantId.Value, cancellationToken));
    }

    private async Task<string?> GetTimeZoneIdAsync(Guid tenantId, CancellationToken ct)
    {
        var key = $"tenant-timezone:{tenantId}";
        if (_cache is not null && _cache.TryGetValue(key, out string? cached))
            return cached;

        var timeZoneId = await _context.TenantSettings
            .AsNoTracking()
            .Where(s => s.TenantId == tenantId)
            .Select(s => s.TimeZone)
            .FirstOrDefaultAsync(ct);

        _cache?.Set(key, timeZoneId, CacheFor);
        return timeZoneId;
    }
}
