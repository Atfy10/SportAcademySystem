using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using Microsoft.AspNetCore.Authorization.Policy;
using SportAcademy.Application.Interfaces;
using SportAcademy.Domain.Entities.Tenants;
using SportAcademy.Domain.Enums;

namespace SportAcademy.Web.Authorization
{
    // Records a Denied TenantAuditEvent the moment an authenticated user is refused a platform
    // route (F-08: "nothing is written when an action is refused"). PermissionAuthorizationHandler
    // can't do this job itself - every platform route today is gated by
    // [Authorize(Roles="SuperAdmin")], a built-in role check ASP.NET Core's own
    // RolesAuthorizationRequirement handles, never reaching that class's custom
    // PermissionRequirement at all. This is the one place both kinds of failure - role-based
    // today, permission-based once Phase 4 adds those policies - surface uniformly, since
    // IAuthorizationMiddlewareResultHandler sees the finished PolicyAuthorizationResult
    // regardless of which requirement produced it.
    //
    // Decorates (rather than replaces) the framework's default handler, which still has to run -
    // it's what actually writes the 403/challenge response.
    public class PlatformDenialAuditResultHandler : IAuthorizationMiddlewareResultHandler
    {
        private const string PlatformPathPrefix = "/api/platform";

        private readonly AuthorizationMiddlewareResultHandler _defaultHandler = new();
        private readonly ILogger<PlatformDenialAuditResultHandler> _logger;

        public PlatformDenialAuditResultHandler(ILogger<PlatformDenialAuditResultHandler> logger)
        {
            _logger = logger;
        }

        public async Task HandleAsync(
            RequestDelegate next,
            HttpContext context,
            AuthorizationPolicy policy,
            PolicyAuthorizationResult authorizeResult)
        {
            if (!authorizeResult.Succeeded
                && context.User.Identity?.IsAuthenticated == true
                && context.Request.Path.StartsWithSegments(PlatformPathPrefix))
            {
                // Audit logging must never be able to break the actual authorization response -
                // this only ever adds a record, and a bug in it must fail silently (logged) here
                // rather than turn into a 500 on every denied platform request.
                try
                {
                    await WriteDeniedEventAsync(context, policy);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to write a Denied platform audit event for {Path}", context.Request.Path);
                }
            }

            await _defaultHandler.HandleAsync(next, context, policy, authorizeResult);
        }

        private static async Task WriteDeniedEventAsync(HttpContext context, AuthorizationPolicy policy)
        {
            var userContext = context.RequestServices.GetRequiredService<IUserContextService>();
            var userId = userContext.UserId;
            if (userId is null) return;

            var auditRepository = context.RequestServices.GetRequiredService<ITenantAuditRepository>();

            var requiredRoles = policy.Requirements
                .OfType<RolesAuthorizationRequirement>()
                .SelectMany(r => r.AllowedRoles)
                .Distinct()
                .ToList();

            var reason = requiredRoles.Count > 0
                ? $"Requires role: {string.Join(", ", requiredRoles)}."
                : "Denied by platform authorization policy.";

            await auditRepository.AddAsync(new TenantAuditEvent
            {
                TenantId = null,
                EventType = "platform.access_denied",
                Description = $"Denied {context.Request.Method} {context.Request.Path}.",
                Outcome = AuditOutcome.Denied,
                Reason = reason,
                PerformedByUserId = userId.Value,
                PerformedBy = context.User.Identity?.Name ?? "Unknown",
                IpAddress = userContext.IpAddress,
                UserAgent = userContext.UserAgent,
                CorrelationId = System.Diagnostics.Activity.Current?.TraceId.ToString(),
                PerformedAt = DateTime.UtcNow,
            });
        }
    }
}
