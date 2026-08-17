using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SportAcademy.Domain.Entities.Tenants;
using SportAcademy.Infrastructure.Persistence.DBContext;

namespace SportAcademy.Web.Controllers;

// Dev-only tenant picker for local login tooling. Every action must stay behind the
// IsDevelopment() guard below — this endpoint has no [Authorize] by design (it exists so a
// developer can pick a tenant before they're able to log in) and previously had no
// environment gate at all, so it leaked the full tenant list anonymously in production.
[ApiController]
[Route("api/[controller]")]
public class DevController : ControllerBase
{
    private readonly ApplicationDbContext _db;
    private readonly IHostEnvironment _env;

    public DevController(ApplicationDbContext db, IHostEnvironment env)
    {
        _db = db;
        _env = env;
    }

    [HttpGet("tenants")]
    public async Task<IActionResult> GetTenants(CancellationToken ct)
    {
        if (!_env.IsDevelopment())
        {
            return NotFound();
        }

        var tenants = await _db.Set<Tenant>()
            .AsNoTracking()
            .OrderBy(t => t.Name)
            .Select(t => new
            {
                t.Id,
                t.Name,
                t.DisplayName,
                t.Slug
            })
            .ToListAsync(ct);

        return Ok(new
        {
            data = tenants,
            isSuccess = true,
            statusCode = 200
        });
    }
}
