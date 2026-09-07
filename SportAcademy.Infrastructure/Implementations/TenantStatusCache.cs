using Microsoft.Extensions.Caching.Memory;
using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Contract;
using SportAcademy.Domain.Enums;

namespace SportAcademy.Infrastructure.Implementations
{
    // Backs TenantStatusGuardMiddleware's per-request status check. Same shape as
    // PermissionResolver: cached per-tenant in IMemoryCache with a sliding TTL, invalidated
    // immediately by any write that changes Tenant.Status so a suspend/archive/restore takes
    // effect on the very next request rather than waiting out the window. IMemoryCache is
    // process-local: correct for the current single-instance deployment; a multi-instance
    // deployment would need a distributed cache (or a much shorter TTL) so one instance's
    // Invalidate() reaches the others - see PermissionResolver for the identical caveat.
    public class TenantStatusCache : ITenantStatusCache, ITenantStatusCacheInvalidator
    {
        private const string CacheKeyPrefix = "tenant-status:";
        private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(5);

        private readonly ITenantRepository _tenantRepository;
        private readonly IMemoryCache _cache;

        public TenantStatusCache(ITenantRepository tenantRepository, IMemoryCache cache)
        {
            _tenantRepository = tenantRepository;
            _cache = cache;
        }

        public async Task<TenantStatus?> GetStatusAsync(Guid tenantId, CancellationToken ct = default)
        {
            var cacheKey = CacheKeyPrefix + tenantId;
            if (_cache.TryGetValue(cacheKey, out TenantStatus? cached))
                return cached;

            var tenant = await _tenantRepository.GetByIdAsync(tenantId, ct);
            var status = tenant?.Status;

            _cache.Set(cacheKey, status, new MemoryCacheEntryOptions
            {
                SlidingExpiration = CacheDuration,
            });

            return status;
        }

        public void Invalidate(Guid tenantId) => _cache.Remove(CacheKeyPrefix + tenantId);
    }
}
