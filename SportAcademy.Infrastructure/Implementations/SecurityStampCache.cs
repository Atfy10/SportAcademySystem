using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using SportAcademy.Application.Interfaces;
using SportAcademy.Infrastructure.Persistence.DBContext;

namespace SportAcademy.Infrastructure.Implementations
{
    // Backs AccessTokenSessionValidator's per-request access-token check. Same shape as
    // TenantStatusCache: cached per-user in IMemoryCache with a sliding TTL, and invalidated
    // immediately by ISessionRevocationService the moment it rotates a stamp, so a revoked
    // session is refused on the very next request rather than after the window. IMemoryCache is
    // process-local - correct for the current single-instance deployment (see TenantStatusCache
    // for the identical caveat).
    public class SecurityStampCache : ISecurityStampCache, ISecurityStampCacheInvalidator
    {
        private const string CacheKeyPrefix = "user-session:";
        private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(5);

        private readonly ApplicationDbContext _context;
        private readonly IMemoryCache _cache;

        public SecurityStampCache(ApplicationDbContext context, IMemoryCache cache)
        {
            _context = context;
            _cache = cache;
        }

        public async Task<UserSessionState?> GetAsync(Guid userId, CancellationToken ct = default)
        {
            var cacheKey = CacheKeyPrefix + userId;
            if (_cache.TryGetValue(cacheKey, out UserSessionState? cached))
                return cached;

            // IgnoreQueryFilters: this runs inside authentication, before the request's ambient
            // tenant is set, so the tenant filter would match nothing - and the soft-delete
            // filter would hide exactly the deleted user this check has to refuse.
            var state = await _context.Users
                .IgnoreQueryFilters()
                .AsNoTracking()
                .Where(u => u.Id == userId)
                .Select(u => new UserSessionState(u.SecurityStamp, u.IsBanned, u.IsDeleted))
                .FirstOrDefaultAsync(ct);

            _cache.Set(cacheKey, state, new MemoryCacheEntryOptions
            {
                SlidingExpiration = CacheDuration,
            });

            return state;
        }

        public void Invalidate(Guid userId) => _cache.Remove(CacheKeyPrefix + userId);
    }
}
