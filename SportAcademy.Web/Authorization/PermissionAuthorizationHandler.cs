using Microsoft.AspNetCore.Authorization;

namespace SportAcademy.Web.Authorization
{
    public class PermissionAuthorizationHandler : AuthorizationHandler<PermissionRequirement>
    {
        protected override Task HandleRequirementAsync(
            AuthorizationHandlerContext context, PermissionRequirement requirement)
        {
            // SuperAdmin/Owner/Admin are seeded with every permission in AppDataSeeder, so
            // this is just "does the token carry the claim" - no role special-casing needed
            // here, keeping this handler a single, uniform rule.
            if (context.User.HasClaim("permission", requirement.Permission))
            {
                context.Succeed(requirement);
            }

            return Task.CompletedTask;
        }
    }
}
