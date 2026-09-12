using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SportAcademy.Application.Commands.PlatformCommands.UpdateLead;
using SportAcademy.Application.Queries.PlatformQueries.GetLeadDetails;
using SportAcademy.Application.Queries.PlatformQueries.GetLeads;

namespace SportAcademy.Web.Controllers.Platform;

[Authorize(Roles = "SuperAdmin")]
// per-user, not per-tenant: see TenantsController for why (F-11) - every SuperAdmin shares
// the System tenant's tenant_id claim.
[EnableRateLimiting("per-user")]
[Route("api/platform/leads")]
[ApiController]
public class LeadsController : ControllerBase
{
    private readonly IMediator _mediator;

    public LeadsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    [Authorize(Policy = "Permission:platform.leads.read")]
    public async Task<IActionResult> GetLeads(
        [FromQuery] int? page,
        [FromQuery] int? pageSize,
        [FromQuery] string? status,
        [FromQuery] string? search,
        CancellationToken ct)
    {
        var result = await _mediator.Send(new GetLeadsQuery(page, pageSize, status, search), ct);
        return StatusCode(result.StatusCode, result);
    }

    [HttpGet("{id}")]
    [Authorize(Policy = "Permission:platform.leads.read")]
    public async Task<IActionResult> GetLead([FromRoute] Guid id, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetLeadDetailsQuery(id), ct);
        return StatusCode(result.StatusCode, result);
    }

    [HttpPatch("{id}")]
    [Authorize(Policy = "Permission:platform.leads.manage")]
    public async Task<IActionResult> UpdateLead(
        [FromRoute] Guid id,
        [FromBody] UpdateLeadRequest request,
        CancellationToken ct)
    {
        var result = await _mediator.Send(
            new UpdateLeadCommand(id, request.Status, request.InternalNotes, request.ConvertedTenantId), ct);
        return StatusCode(result.StatusCode, result);
    }
}

public record UpdateLeadRequest(string Status, string? InternalNotes, Guid? ConvertedTenantId = null);
