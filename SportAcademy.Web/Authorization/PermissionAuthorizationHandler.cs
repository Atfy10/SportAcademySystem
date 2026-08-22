using Microsoft.AspNetCore.Authorization;
using SportAcademy.Application.Interfaces;
using System.Security.Claims;

namespace SportAcademy.Web.Authorization
{
    public class PermissionAuthorizationHandler : AuthorizationHandler<PermissionRequirement>
    {
        private readonly IPermissionResolver _permissionResolver;

        public PermissionAuthorizationHandler(IPermissionResolver permissionResolver)
        {
            _permissionResolver = permissionResolver;
        }

        // Resolved server-side on every check rather than read off the token's "permission"
        // claims - those claims are stamped at token-issue time and can be up to
        // Jwt:ExpireMinutes stale, which would make an explicit Deny meaningless for however
        // long the caller's current access token remains valid. Going through the resolver
        // (which is cached and invalidated on write, see PermissionResolver) keeps a Deny
        // effective within seconds instead of up to 30 minutes.
        protected override async Task HandleRequirementAsync(
            AuthorizationHandlerContext context, PermissionRequirement requirement)
        {
            var userIdClaim = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userIdClaim is null || !Guid.TryParse(userIdClaim, out var userId))
                return;

            if (await _permissionResolver.HasPermissionAsync(userId, requirement.Permission))
            {
                context.Succeed(requirement);
            }
        }
    }
}
