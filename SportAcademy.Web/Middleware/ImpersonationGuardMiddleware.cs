using System.Text.Json;
using SportAcademy.Application.Interfaces;

namespace SportAcademy.Web.Middleware
{
    // Enforces the two guarantees an impersonation session promises (Phase 3 of the platform
    // hardening work): read-only, and revocable before its own natural JWT expiry.
    //
    // Natural expiry needs no code here at all - the token's own signed `exp` claim already
    // makes JwtBearer's authentication step reject it, same as any other token, so a request on
    // an expired impersonation token never reaches this far with the claim still attached. What
    // this middleware adds is the thing a JWT can't do on its own: notice an *early*, deliberate
    // "End session" before that natural expiry.
    //
    // Every request under an active grant is logged (method + path + grant + tenant) via the
    // structured application log, not the platform TenantAuditEvent table - that table is for
    // discrete actions, and every GET a browsing session makes would be noise there.
    public class ImpersonationGuardMiddleware
    {
        // Platform routes are the control plane for impersonation itself (starting, ending,
        // and everything else about running the platform console) - never subject to the
        // read-only restriction the rest of this middleware enforces. Same prefix
        // TenantStatusGuardMiddleware exempts, for the same reason: this class only ever
        // applies to a tenant's own business routes.
        private static readonly string[] ExemptPathPrefixes = ["/api/platform", "/api/auth"];

        private static readonly HashSet<string> ReadOnlyMethods = new(StringComparer.OrdinalIgnoreCase)
        {
            HttpMethods.Get, HttpMethods.Head, HttpMethods.Options,
        };

        private readonly RequestDelegate _next;
        private readonly ILogger<ImpersonationGuardMiddleware> _logger;

        public ImpersonationGuardMiddleware(RequestDelegate next, ILogger<ImpersonationGuardMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(
            HttpContext context, IUserContextService userContext, IImpersonationGrantRepository grantRepository)
        {
            var grantId = userContext.ImpersonationGrantId;
            if (grantId is null)
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

            if (!ReadOnlyMethods.Contains(context.Request.Method))
            {
                _logger.LogWarning(
                    "Blocked write attempt under impersonation grant {GrantId}: {Method} {Path}",
                    grantId, context.Request.Method, path);

                await WriteRejectionAsync(context, 403, "IMPERSONATION_READ_ONLY",
                    "Impersonation sessions are read-only.");
                return;
            }

            var grant = await grantRepository.GetByIdAsync(grantId.Value, context.RequestAborted);
            if (grant is null || grant.EndedAt is not null)
            {
                await WriteRejectionAsync(context, 401, "IMPERSONATION_ENDED",
                    "This impersonation session has ended.");
                return;
            }

            _logger.LogInformation(
                "Impersonation grant {GrantId} (tenant {TenantId}): {Method} {Path}",
                grantId, grant.TenantId, context.Request.Method, path);

            await _next(context);
        }

        private static async Task WriteRejectionAsync(HttpContext context, int statusCode, string code, string message)
        {
            context.Response.StatusCode = statusCode;
            context.Response.ContentType = "application/json";

            var payload = new
            {
                isSuccess = false,
                operationType = "ImpersonationGuard",
                statusCode,
                message,
                errors = new { code },
            };

            await context.Response.WriteAsync(JsonSerializer.Serialize(payload));
        }
    }
}
