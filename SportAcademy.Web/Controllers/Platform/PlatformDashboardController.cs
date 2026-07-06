using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SportAcademy.Application.Queries.PlatformQueries.GetPlatformDashboard;

namespace SportAcademy.Web.Controllers.Platform;

[Authorize(Roles = "SuperAdmin")]
[EnableRateLimiting("per-tenant")]
[Route("api/platform/dashboard")]
[ApiController]
public class PlatformDashboardController : ControllerBase
{
    private readonly IMediator _mediator;

    public PlatformDashboardController(IMediator mediator) => _mediator = mediator;

    [HttpGet]
    public async Task<IActionResult> GetDashboard(CancellationToken ct)
    {
        var result = await _mediator.Send(new GetPlatformDashboardQuery(), ct);
        return Ok(result);
    }
}
