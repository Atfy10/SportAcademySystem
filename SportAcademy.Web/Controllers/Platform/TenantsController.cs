using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SportAcademy.Application.Commands.PlatformCommands.ArchiveTenant;
using SportAcademy.Application.Commands.PlatformCommands.ChangeTenantPlan;
using SportAcademy.Application.Commands.PlatformCommands.ChangeTenantStatus;
using SportAcademy.Application.Commands.PlatformCommands.CreateTenant;
using SportAcademy.Application.Commands.PlatformCommands.ExpireTenantSubscription;
using SportAcademy.Application.Commands.PlatformCommands.ExtendTenantSubscription;
using SportAcademy.Application.Commands.PlatformCommands.SetTenantTrial;
using SportAcademy.Application.Commands.PlatformCommands.ToggleFeature;
using SportAcademy.Application.Commands.PlatformCommands.UpdateTenant;
using SportAcademy.Application.Queries.PlatformQueries.GetTenantDetails;
using SportAcademy.Application.Queries.PlatformQueries.GetTenantFeatures;
using SportAcademy.Application.Queries.PlatformQueries.GetTenants;
using SportAcademy.Domain.Enums;

namespace SportAcademy.Web.Controllers.Platform;

[Authorize(Roles = "SuperAdmin")]
[EnableRateLimiting("per-tenant")]
[Route("api/platform/tenants")]
[ApiController]
public class TenantsController : ControllerBase
{
    private readonly IMediator _mediator;

    public TenantsController(IMediator mediator) => _mediator = mediator;

    [HttpGet]
    public async Task<IActionResult> GetTenants(
        [FromQuery] int? page,
        [FromQuery] int? pageSize,
        [FromQuery] string? status,
        [FromQuery] string? search,
        CancellationToken ct)
    {
        var result = await _mediator.Send(
            new GetTenantsQuery(page, pageSize, status, search), ct);
        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> CreateTenant(
        [FromBody] CreateTenantRequest request,
        CancellationToken ct)
    {
        var command = new CreateTenantCommand(
            request.Name,
            request.DisplayName,
            request.Slug,
            request.Code,
            request.Email,
            request.OwnerName,
            request.OwnerEmail,
            request.SubscriptionPlanId,
            request.Phone,
            request.Address,
            request.TimeZone,
            request.Language,
            request.Currency);

        var result = await _mediator.Send(command, ct);
        return StatusCode(result.StatusCode, result);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetTenantDetails(
        [FromRoute] Guid id,
        CancellationToken ct)
    {
        var result = await _mediator.Send(new GetTenantDetailsQuery(id), ct);
        return StatusCode(result.StatusCode, result);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateTenant(
        [FromRoute] Guid id,
        [FromBody] UpdateTenantRequest request,
        CancellationToken ct)
    {
        var command = new UpdateTenantCommand(
            id,
            request.Name,
            request.DisplayName,
            request.Email,
            request.Phone,
            request.Address,
            request.Website,
            request.Description,
            request.TimeZone,
            request.Language,
            request.Currency);

        var result = await _mediator.Send(command, ct);
        return StatusCode(result.StatusCode, result);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> ArchiveTenant(
        [FromRoute] Guid id,
        CancellationToken ct)
    {
        var result = await _mediator.Send(new ArchiveTenantCommand(id), ct);
        return StatusCode(result.StatusCode, result);
    }

    [HttpPatch("{id}/status")]
    public async Task<IActionResult> ChangeTenantStatus(
        [FromRoute] Guid id,
        [FromBody] ChangeTenantStatusRequest request,
        CancellationToken ct)
    {
        var result = await _mediator.Send(
            new ChangeTenantStatusCommand(id, request.NewStatus), ct);
        return StatusCode(result.StatusCode, result);
    }

    [HttpPatch("{id}/plan")]
    public async Task<IActionResult> ChangeTenantPlan(
        [FromRoute] Guid id,
        [FromBody] ChangeTenantPlanRequest request,
        CancellationToken ct)
    {
        var result = await _mediator.Send(
            new ChangeTenantPlanCommand(id, request.NewPlanId), ct);
        return StatusCode(result.StatusCode, result);
    }

    [HttpGet("{id}/features")]
    public async Task<IActionResult> GetTenantFeatures(
        [FromRoute] Guid id,
        CancellationToken ct)
    {
        var result = await _mediator.Send(new GetTenantFeaturesQuery(id), ct);
        return StatusCode(result.StatusCode, result);
    }

    [HttpPatch("{id}/features")]
    public async Task<IActionResult> ToggleFeature(
        [FromRoute] Guid id,
        [FromBody] ToggleFeatureRequest request,
        CancellationToken ct)
    {
        var result = await _mediator.Send(
            new ToggleFeatureCommand(id, request.FeatureId, request.IsEnabled), ct);
        return StatusCode(result.StatusCode, result);
    }

    [HttpPatch("{id}/subscription")]
    public async Task<IActionResult> ExtendSubscription(
        [FromRoute] Guid id,
        [FromBody] ExtendSubscriptionRequest request,
        CancellationToken ct)
    {
        var result = await _mediator.Send(
            new ExtendTenantSubscriptionCommand(id, request.Days), ct);
        return StatusCode(result.StatusCode, result);
    }

    [HttpPost("{id}/subscription/expire")]
    public async Task<IActionResult> ExpireSubscription(
        [FromRoute] Guid id,
        CancellationToken ct)
    {
        var result = await _mediator.Send(
            new ExpireTenantSubscriptionCommand(id), ct);
        return StatusCode(result.StatusCode, result);
    }

    [HttpPost("{id}/subscription/trial")]
    public async Task<IActionResult> SetTrial(
        [FromRoute] Guid id,
        CancellationToken ct)
    {
        var result = await _mediator.Send(
            new SetTenantTrialCommand(id), ct);
        return StatusCode(result.StatusCode, result);
    }
}

public record CreateTenantRequest(
    string Name,
    string DisplayName,
    string Slug,
    string Code,
    string Email,
    string OwnerName,
    string OwnerEmail,
    int SubscriptionPlanId,
    string? Phone = null,
    string? Address = null,
    string? TimeZone = null,
    string? Language = null,
    string? Currency = null);

public record UpdateTenantRequest(
    string? Name = null,
    string? DisplayName = null,
    string? Email = null,
    string? Phone = null,
    string? Address = null,
    string? Website = null,
    string? Description = null,
    string? TimeZone = null,
    string? Language = null,
    string? Currency = null);

public record ChangeTenantStatusRequest(TenantStatus NewStatus);

public record ChangeTenantPlanRequest(int NewPlanId);

public record ToggleFeatureRequest(Guid FeatureId, bool IsEnabled);

public record ExtendSubscriptionRequest(int Days);
