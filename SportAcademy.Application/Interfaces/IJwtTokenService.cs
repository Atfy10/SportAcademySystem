using SportAcademy.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SportAcademy.Application.Interfaces
{
    public interface IJwtTokenService
    {
        Task<string> GenerateJwtToken(AppUser appUser, params string[] roles);

        /// <summary>A short-lived access token for a SuperAdmin's impersonation session: same
        /// claim shape as GenerateJwtToken, but tenant_id is the tenant being accessed (not the
        /// SuperAdmin's own), sub/NameIdentifier stays the SuperAdmin's real user id (so
        /// anything checking "who is this" - PermissionResolver included - still resolves the
        /// real actor), and an extra impersonation_grant_id claim marks it for
        /// ImpersonationGuardMiddleware. Expires exactly at expiresAt - there is no refresh path
        /// for this token; a session that runs out is over. No role/permission narrowing happens
        /// here - see ImpersonationGuardMiddleware for how read-only is actually enforced.</summary>
        Task<string> GenerateImpersonationToken(
            AppUser superAdmin, Guid targetTenantId, Guid grantId, DateTime expiresAt);

        string GenerateRefreshToken();
        string HashToken(string token);
        Task<RefreshTokenResult?> ValidateAndRefreshTokenAsync(string plainRefreshToken, CancellationToken ct = default);
        Task<RefreshToken?> GetRefreshTokenByHashAsync(string tokenHash, CancellationToken ct = default);
        Task RevokeRefreshTokenAsync(RefreshToken token, CancellationToken ct = default);
    }

    public record RefreshTokenResult(string AccessToken, string RefreshToken);
}
