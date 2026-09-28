using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SportAcademy.Application.Commands.PlatformCommands.SeedDemoData;

namespace SportAcademy.Web.Controllers.Platform;

// Development tooling: seeds the fictional demo tenant on demand instead of at startup, so a
// fresh database can be used empty first. SuperAdmin-only like the rest of the platform console;
// tenant-manage is the closest existing permission (this creates a tenant), so no new
// permission - and therefore no migration to grant it - is needed for a dev-only action.
// Outside the Development environment the handler refuses with 403 errors.demoData.notDevelopment.
[Authorize(Roles = "SuperAdmin")]
// per-user, not per-tenant: see TenantsController for why (F-11).
[EnableRateLimiting("per-user")]
[Route("api/platform/demo-data")]
[ApiController]
[Authorize(Policy = "Permission:platform.tenants.manage")]
public class DemoDataController : ControllerBase
{
    private readonly IMediator _mediator;

    public DemoDataController(IMediator mediator) => _mediator = mediator;

    [HttpPost("seed")]
    public async Task<IActionResult> Seed(CancellationToken ct)
    {
        var result = await _mediator.Send(new SeedDemoDataCommand(), ct);
        return StatusCode(result.StatusCode, result);
    }
}
