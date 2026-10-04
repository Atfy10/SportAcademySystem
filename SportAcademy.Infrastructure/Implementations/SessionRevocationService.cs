using Microsoft.AspNetCore.Identity;
using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Contract;
using SportAcademy.Domain.Entities;

namespace SportAcademy.Infrastructure.Implementations
{
    // See ISessionRevocationService for what this covers and why each step is needed.
    public class SessionRevocationService : ISessionRevocationService
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly ITenantIdProvider _tenantIdProvider;
        private readonly IRefreshTokenRepository _refreshTokenRepository;
        private readonly ISecurityStampCacheInvalidator _securityStampCache;
        private readonly IPermissionCacheInvalidator _permissionCache;
        private readonly IRealtimeService _realtimeService;

        public SessionRevocationService(
            UserManager<AppUser> userManager,
            ITenantIdProvider tenantIdProvider,
            IRefreshTokenRepository refreshTokenRepository,
            ISecurityStampCacheInvalidator securityStampCache,
            IPermissionCacheInvalidator permissionCache,
            IRealtimeService realtimeService)
        {
            _userManager = userManager;
            _tenantIdProvider = tenantIdProvider;
            _refreshTokenRepository = refreshTokenRepository;
            _securityStampCache = securityStampCache;
            _permissionCache = permissionCache;
            _realtimeService = realtimeService;
        }

        public async Task RevokeAllSessionsAsync(AppUser user, string reason, CancellationToken ct = default)
        {
            // AppUser is ITenantScoped, and the caller is not always in the user's own tenant:
            // the reset-password link is anonymous (no ambient tenant at all) and BanOwner runs
            // as a SuperAdmin (System tenant). Align the ambient tenant to the user's own for
            // this one write - same technique as BanOwnerCommandHandler - and restore it after.
            using (_tenantIdProvider.Impersonate(user.TenantId))
            {
                var result = await _userManager.UpdateSecurityStampAsync(user);
                if (!result.Succeeded)
                    throw new InvalidOperationException(
                        "Failed to rotate the security stamp: " +
                        string.Join(" ", result.Errors.Select(e => e.Description)));
            }

            await _refreshTokenRepository.RevokeAllUserTokensAsync(user.Id, ct);

            _securityStampCache.Invalidate(user.Id);
            _permissionCache.Invalidate(user.Id);

            await _realtimeService.NotifySessionRevokedAsync(user.Id, reason);
        }
    }
}
