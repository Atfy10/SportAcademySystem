using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SportAcademy.Application.Commands.PlatformCommands.BanOwner;
using SportAcademy.Application.Commands.PlatformCommands.SendOwnerPasswordResetLink;
using SportAcademy.Application.Queries.PlatformQueries.GetOwnerById;
using SportAcademy.Application.Queries.PlatformQueries.GetOwners;

namespace SportAcademy.Web.Controllers.Platform;

[Authorize(Roles = "SuperAdmin")]
// per-user, not per-tenant: see TenantsController for why (F-11).
[EnableRateLimiting("per-user")]
[Route("api/platform/owners")]
[ApiController]
// Owners has a single permission (Manage) rather than splitting read/write like Tenants -
// there's no read-only "view owners" use case distinct from the ability to ban/reset them,
// so a lone platform.owners.manage covers this whole controller.
[Authorize(Policy = "Permission:platform.owners.manage")]
public class OwnersController : ControllerBase
{
    private readonly IMediator _mediator;

    public OwnersController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<IActionResult> GetOwners(
        [FromQuery] int? page,
        [FromQuery] int? pageSize,
        [FromQuery] string? search,
        CancellationToken ct)
    {
        var result = await _mediator.Send(new GetOwnersQuery(page, pageSize, search), ct);
        return StatusCode(result.StatusCode, result);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetOwnerById(
        [FromRoute] Guid id,
        CancellationToken ct)
    {
        var result = await _mediator.Send(new GetOwnerByIdQuery(id), ct);
        return StatusCode(result.StatusCode, result);
    }

    [HttpPatch("{id}/ban")]
    public async Task<IActionResult> BanOwner(
        [FromRoute] Guid id,
        [FromBody] BanOwnerRequest request,
        CancellationToken ct)
    {
        var result = await _mediator.Send(new BanOwnerCommand(id, request.Banned), ct);
        return StatusCode(result.StatusCode, result);
    }

    [HttpPost("{id}/send-reset-link")]
    public async Task<IActionResult> SendResetLink(
        [FromRoute] Guid id,
        CancellationToken ct)
    {
        var result = await _mediator.Send(new SendOwnerPasswordResetLinkCommand(id), ct);
        return StatusCode(result.StatusCode, result);
    }
}

public record BanOwnerRequest(bool Banned);
