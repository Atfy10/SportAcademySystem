using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SportAcademy.Domain.Entities.Tenants;
using SportAcademy.Infrastructure.Persistence.DBContext;

namespace SportAcademy.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DevController : ControllerBase
{
    private readonly ApplicationDbContext _db;

    public DevController(ApplicationDbContext db)
    {
        _db = db;
    }

    [HttpGet("tenants")]
    public async Task<IActionResult> GetTenants(CancellationToken ct)
    {
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
