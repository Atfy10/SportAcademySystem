using SportAcademy.Application.Interfaces;
using SportAcademy.Infrastructure.Implementations;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace SportAcademy.Web.Authorization
{
    // Run from JwtBearerEvents.OnTokenValidated (Program.cs) on every authenticated request,
    // including the SignalR negotiate. A signature-valid, unexpired access token is still
    // refused once the user's SecurityStamp has moved on from the one baked into it - that is
    // what lets ISessionRevocationService end every session at once instead of waiting out the
    // token lifetime. Also refuses a banned or deleted user's token outright.
    public static class AccessTokenSessionValidator
    {
        public const string FailureMessage = "SESSION_REVOKED";

        public static async Task<bool> IsSessionValidAsync(
            ClaimsPrincipal principal, ISecurityStampCache cache, CancellationToken ct = default)
        {
            var userIdValue = principal.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? principal.FindFirstValue(JwtRegisteredClaimNames.Sub);
            if (!Guid.TryParse(userIdValue, out var userId))
                return false;

            var state = await cache.GetAsync(userId, ct);
            if (state is null || state.IsBanned || state.IsDeleted)
                return false;

            // Both sides default to "" rather than failing on a missing value: a token issued
            // before the claim existed gets one 401 for a user who has a stamp, and the client's
            // silent refresh then mints one that carries it - nobody is forced to log in again.
            // A legacy account with no stamp at all (and so an empty claim) still gets in, and is
            // locked out the moment a revocation gives it a real stamp.
            var tokenStamp = principal.FindFirstValue(JwtTokenService.SecurityStampClaimType) ?? string.Empty;
            return string.Equals(state.SecurityStamp ?? string.Empty, tokenStamp, StringComparison.Ordinal);
        }
    }
}
