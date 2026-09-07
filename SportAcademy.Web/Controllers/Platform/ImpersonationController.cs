using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SportAcademy.Application.Commands.PlatformCommands.EndImpersonation;

namespace SportAcademy.Web.Controllers.Platform;

// Separate from TenantsController (which starts a session, under /tenants/{id}/impersonate)
// because ending one only needs the grant id, not a tenant route segment - and because this is
// reachable using the impersonation token itself (it carries role=SuperAdmin, same as the
// role check here), which is what lets the frontend's own "End session" button work while
// still inside the impersonated view.
[Authorize(Roles = "SuperAdmin")]
// The impersonation JWT itself carries role=SuperAdmin and is issued with that role's current
// permission claims (see JwtTokenService.BuildTokenAsync), so this policy check still passes
// when End() is called using the impersonation token, not just the SuperAdmin's own.
[Authorize(Policy = "Permission:platform.impersonate")]
[EnableRateLimiting("per-user")]
[Route("api/platform/impersonation")]
[ApiController]
public class ImpersonationController : ControllerBase
{
    private readonly IMediator _mediator;

    public ImpersonationController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("{grantId}/end")]
    public async Task<IActionResult> End([FromRoute] Guid grantId, CancellationToken ct)
    {
        var result = await _mediator.Send(new EndImpersonationCommand(grantId), ct);
        return StatusCode(result.StatusCode, result);
    }
}
