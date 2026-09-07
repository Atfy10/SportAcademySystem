using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Enums;

namespace SportAcademy.Web.Middleware
{
    // Uploaded files (avatars, employee/trainee photos, tenant logos) are served by plain
    // UseStaticFiles() with no auth check at all, by design - these need to stay publicly
    // viewable wherever the app renders them, the same as any CDN-hosted image would be. But a
    // suspended/archived tenant's files must not remain reachable just because the URL is
    // unguessable. This reads the tenant id LocalFileStorageService already embeds in every
    // upload path (/uploads/{tenantId:N}/...) and checks it against the same status cache
    // TenantStatusGuardMiddleware already maintains - registered immediately before
    // UseStaticFiles() so a blocked request never reaches the file handler at all.
    public class TenantFileAccessGuardMiddleware
    {
        private readonly RequestDelegate _next;

        public TenantFileAccessGuardMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context, ITenantStatusCache tenantStatusCache)
        {
            var segments = context.Request.Path.Value?.Split('/', StringSplitOptions.RemoveEmptyEntries);

            // Not an /uploads/{tenantId}/... request (or a malformed/pre-migration path with no
            // tenant segment) - nothing this middleware can check, let it through as before.
            if (segments is not { Length: >= 2 }
                || !string.Equals(segments[0], "uploads", StringComparison.OrdinalIgnoreCase)
                || !Guid.TryParseExact(segments[1], "N", out var tenantId))
            {
                await _next(context);
                return;
            }

            var status = await tenantStatusCache.GetStatusAsync(tenantId, context.RequestAborted);

            // A tenant that has vanished is treated the same as not-Active rather than let
            // through, mirroring TenantStatusGuardMiddleware's identical choice.
            if (status is null or not TenantStatus.Active)
            {
                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                return;
            }

            await _next(context);
        }
    }
}
