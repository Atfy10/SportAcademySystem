using System.Text.Json;
using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Enums;

namespace SportAcademy.Web.Middleware
{
    // Every platform-console control that stops a tenant (suspend, deactivate, archive) used to
    // write Tenant.Status and nothing else - no middleware read it, so a suspended tenant's
    // users kept working normally on their existing token, and could still log in for a brand
    // new one. This is the enforcement point: it runs for every authenticated, tenant-scoped
    // request and rejects anything but Active.
    //
    // Placed after UseAuthentication() (needs the tenant_id claim) and before UseAuthorization()
    // (a suspended tenant should get 403 TENANT_SUSPENDED, not a permission-shaped 403). Reads
    // IUserContextService directly rather than ITenantIdProvider, so it does not depend on the
    // inline middleware in Program.cs that populates the latter - it would work whichever side
    // of that block it runs on.
    public class TenantStatusGuardMiddleware
    {
        // Auth itself must stay reachable so a locked-out user can be told why (and so refresh/
        // logout/revoke keep working). Platform routes are the SuperAdmin's own console and are
        // never tenant-scoped in the sense this guard cares about. /health has no tenant at all.
        private static readonly string[] ExemptPathPrefixes =
        [
            "/api/auth",
            "/api/platform",
            "/health",
        ];

        private readonly RequestDelegate _next;

        public TenantStatusGuardMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(
            HttpContext context, IUserContextService userContext, ITenantStatusCache tenantStatusCache)
        {
            if (!userContext.IsAuthenticated || userContext.TenantId is not { } tenantId)
            {
                await _next(context);
                return;
            }

            var path = context.Request.Path.Value ?? string.Empty;
            if (ExemptPathPrefixes.Any(prefix => path.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)))
            {
                await _next(context);
                return;
            }

            var status = await tenantStatusCache.GetStatusAsync(tenantId, context.RequestAborted);

            // A tenant that has vanished (should be unreachable - a valid token always carries
            // a real tenant id) is treated the same as Archived rather than let through.
            if (status is null or not TenantStatus.Active)
            {
                var code = status == TenantStatus.Archived ? "TENANT_ARCHIVED" : "TENANT_SUSPENDED";
                var message = status == TenantStatus.Archived
                    ? "This academy's account has been archived."
                    : "This academy's account is currently suspended.";

                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                context.Response.ContentType = "application/json";

                var payload = new
                {
                    isSuccess = false,
                    operationType = "TenantStatusGuard",
                    statusCode = StatusCodes.Status403Forbidden,
                    message,
                    errors = new { code },
                };

                await context.Response.WriteAsync(JsonSerializer.Serialize(payload));
                return;
            }

            await _next(context);
        }
    }
}
