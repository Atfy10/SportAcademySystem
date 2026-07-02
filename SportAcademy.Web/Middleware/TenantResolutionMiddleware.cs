using Microsoft.EntityFrameworkCore;
using SportAcademy.Domain.Contract;
using SportAcademy.Infrastructure.Persistence.DBContext;

namespace SportAcademy.Web.Middleware;

public class TenantResolutionMiddleware
{
    private readonly RequestDelegate _next;

    public TenantResolutionMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, ApplicationDbContext dbContext, ITenantIdProvider tenantIdProvider)
    {
        var path = context.Request.Path.Value;

        if (path is not null)
        {
            var match = System.Text.RegularExpressions.Regex.Match(
                path, @"^/t/([^/]+)", System.Text.RegularExpressions.RegexOptions.IgnoreCase);

            if (match.Success)
            {
                var slug = match.Groups[1].Value;

                var tenant = await dbContext.Set<Domain.Entities.Tenants.Tenant>()
                    .AsNoTracking()
                    .FirstOrDefaultAsync(t => t.Slug == slug);

                if (tenant is not null)
                {
                    tenantIdProvider.SetTenantId(tenant.Id);
                    context.Items["ResolvedTenantId"] = tenant.Id;
                    context.Items["ResolvedTenantSlug"] = tenant.Slug;
                }
            }
        }

        await _next(context);
    }
}
