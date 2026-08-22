using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Entities;
using SportAcademy.Domain.Enums;
using SportAcademy.Infrastructure.Persistence.DBContext;

namespace SportAcademy.Infrastructure.Implementations
{
    // Single source of truth for "can this user do X right now". Resolution order:
    //   1. start with the union of permission claims on every role the user holds
    //   2. add any of the user's own overrides with Effect == Allow
    //   3. remove any of the user's own overrides with Effect == Deny  <- always wins
    //
    // Cached per-user for 5 minutes (sliding) in IMemoryCache, but every write path that can
    // change the outcome (permission overrides, role assignment, invitation acceptance, user
    // activation toggle) calls Invalidate() so a change is visible on the very next request
    // instead of waiting out the sliding window. IMemoryCache is process-local: correct for the
    // current single-instance IIS deployment; a multi-instance deployment would need a
    // distributed cache (or a much shorter TTL) so one instance's Invalidate() reaches the
    // others.
    public class PermissionResolver : IPermissionResolver, IPermissionCacheInvalidator
    {
        private const string CacheKeyPrefix = "perm:";
        private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(5);

        private readonly UserManager<AppUser> _userManager;
        private readonly RoleManager<AppRole> _roleManager;
        private readonly ApplicationDbContext _context;
        private readonly IMemoryCache _cache;

        public PermissionResolver(
            UserManager<AppUser> userManager,
            RoleManager<AppRole> roleManager,
            ApplicationDbContext context,
            IMemoryCache cache)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _context = context;
            _cache = cache;
        }

        public async Task<IReadOnlySet<string>> GetEffectivePermissionsAsync(Guid userId, CancellationToken ct = default)
        {
            var cacheKey = CacheKeyPrefix + userId;
            if (_cache.TryGetValue(cacheKey, out HashSet<string>? cached) && cached is not null)
                return cached;

            var effective = await ComputeAsync(userId, ct);

            _cache.Set(cacheKey, effective, new MemoryCacheEntryOptions
            {
                SlidingExpiration = CacheDuration,
            });

            return effective;
        }

        public async Task<bool> HasPermissionAsync(Guid userId, string permission, CancellationToken ct = default)
        {
            var permissions = await GetEffectivePermissionsAsync(userId, ct);
            return permissions.Contains(permission);
        }

        public void Invalidate(Guid userId) => _cache.Remove(CacheKeyPrefix + userId);

        private async Task<HashSet<string>> ComputeAsync(Guid userId, CancellationToken ct)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user is null) return [];

            var roles = await _userManager.GetRolesAsync(user);

            var effective = new HashSet<string>();
            foreach (var roleName in roles)
            {
                var role = await _roleManager.FindByNameAsync(roleName);
                if (role is null) continue;

                var roleClaims = await _roleManager.GetClaimsAsync(role);
                foreach (var claim in roleClaims.Where(c => c.Type == "permission"))
                    effective.Add(claim.Value);
            }

            var overrides = await _context.UserPermissionOverrides
                .Where(o => o.UserId == userId)
                .AsNoTracking()
                .ToListAsync(ct);

            foreach (var o in overrides.Where(o => o.Effect == PermissionEffect.Allow))
                effective.Add(o.Permission);

            foreach (var o in overrides.Where(o => o.Effect == PermissionEffect.Deny))
                effective.Remove(o.Permission);

            return effective;
        }
    }
}
