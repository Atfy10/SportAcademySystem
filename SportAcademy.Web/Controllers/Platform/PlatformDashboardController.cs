using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SportAcademy.Application.Queries.PlatformQueries.GetPlatformDashboard;

namespace SportAcademy.Web.Controllers.Platform;

// Read-only rollup, so PlatformSupport is allowed here unlike the other platform controllers
// whose class-level role check is SuperAdmin-only.
[Authorize(Roles = "SuperAdmin,PlatformSupport")]
// per-user, not per-tenant: see TenantsController for why (F-11).
[EnableRateLimiting("per-user")]
[Route("api/platform/dashboard")]
[ApiController]
[Authorize(Policy = "Permission:platform.tenants.read")]
public class PlatformDashboardController : ControllerBase
{
    private readonly IMediator _mediator;

    public PlatformDashboardController(IMediator mediator) => _mediator = mediator;

    [HttpGet]
    public async Task<IActionResult> GetDashboard(CancellationToken ct)
    {
        var result = await _mediator.Send(new GetPlatformDashboardQuery(), ct);
        return StatusCode(result.StatusCode, result);
    }
}
