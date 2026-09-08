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
using SportAcademy.Application.Commands.PlatformCommands.StartImpersonation;
using SportAcademy.Application.Commands.PlatformCommands.ToggleFeature;
using SportAcademy.Application.Commands.PlatformCommands.UpdatePlanFeatures;
using SportAcademy.Application.Commands.PlatformCommands.UpdateTenant;
using SportAcademy.Application.Queries.PlatformQueries.GetTenantDetails;
using SportAcademy.Application.Queries.PlatformQueries.GetPlanFeatures;
using SportAcademy.Application.Queries.PlatformQueries.GetSubscriptionPlans;
using SportAcademy.Application.Queries.PlatformQueries.GetTenantFeatures;
using SportAcademy.Application.Queries.PlatformQueries.GetTenants;
using SportAcademy.Domain.Enums;

namespace SportAcademy.Web.Controllers.Platform;

// No class-level [Authorize(Roles=...)] here (unlike the other platform controllers) - this
// controller mixes read actions PlatformSupport can reach and mutations only SuperAdmin can,
// so the role check is set per-action instead. Every action still carries its own Roles +
// Permission:platform.* pair; there is no action left ungated by an explicit role check.
// per-user, not per-tenant: every SuperAdmin shares the same tenant_id claim (the System
// tenant), so "per-tenant" would put every platform operator in one shared bucket - one
// runaway script locks out everyone else on the platform console (F-11).
[EnableRateLimiting("per-user")]
[Route("api/platform/tenants")]
[ApiController]
[Authorize]
public class TenantsController : ControllerBase
{
    private readonly IMediator _mediator;

    // No audit call here any more - every command below implements IAuditableCommand, so
    // PlatformAuditBehavior writes exactly one TenantAuditEvent per request, in the same
    // transaction as the handler, without this controller needing to know that auditing exists
    // (see F-05/F-06: the old hand-typed LogAsync call after each Send was easy to duplicate
    // slightly wrong and impossible to make transactional with the handler it was describing).
    public TenantsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    [Authorize(Roles = "SuperAdmin,PlatformSupport")]
    [Authorize(Policy = "Permission:platform.tenants.read")]
    public async Task<IActionResult> GetTenants(
        [FromQuery] int? page,
        [FromQuery] int? pageSize,
        [FromQuery] string? status,
        [FromQuery] string? search,
        CancellationToken ct)
    {
        var result = await _mediator.Send(
            new GetTenantsQuery(page, pageSize, status, search), ct);
        return StatusCode(result.StatusCode, result);
    }

    [HttpPost]
    [Authorize(Roles = "SuperAdmin")]
    [Authorize(Policy = "Permission:platform.tenants.manage")]
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
    [Authorize(Roles = "SuperAdmin,PlatformSupport")]
    [Authorize(Policy = "Permission:platform.tenants.read")]
    public async Task<IActionResult> GetTenantDetails(
        [FromRoute] Guid id,
        CancellationToken ct)
    {
        var result = await _mediator.Send(new GetTenantDetailsQuery(id), ct);
        return StatusCode(result.StatusCode, result);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "SuperAdmin")]
    [Authorize(Policy = "Permission:platform.tenants.manage")]
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
    [Authorize(Roles = "SuperAdmin")]
    [Authorize(Policy = "Permission:platform.tenants.manage")]
    public async Task<IActionResult> ArchiveTenant(
        [FromRoute] Guid id,
        [FromBody] ArchiveTenantRequest request,
        CancellationToken ct)
    {
        var result = await _mediator.Send(new ArchiveTenantCommand(id, request.Reason), ct);
        return StatusCode(result.StatusCode, result);
    }

    [HttpPatch("{id}/status")]
    [Authorize(Roles = "SuperAdmin")]
    [Authorize(Policy = "Permission:platform.tenants.manage")]
    public async Task<IActionResult> ChangeTenantStatus(
        [FromRoute] Guid id,
        [FromBody] ChangeTenantStatusRequest request,
        CancellationToken ct)
    {
        var result = await _mediator.Send(
            new ChangeTenantStatusCommand(id, request.NewStatus, request.Reason), ct);
        return StatusCode(result.StatusCode, result);
    }

    [HttpPatch("{id}/plan")]
    [Authorize(Roles = "SuperAdmin")]
    [Authorize(Policy = "Permission:platform.tenants.manage")]
    public async Task<IActionResult> ChangeTenantPlan(
        [FromRoute] Guid id,
        [FromBody] ChangeTenantPlanRequest request,
        CancellationToken ct)
    {
        var result = await _mediator.Send(
            new ChangeTenantPlanCommand(id, request.NewPlanId), ct);
        return StatusCode(result.StatusCode, result);
    }

    // Absolute route, not tenant-scoped: this is the global plan catalog (name/id/price), not
    // one tenant's own subscription - both the create-tenant and change-plan pickers need it so
    // neither has to hardcode {1,2,3} = {Basic,Pro,Enterprise} and silently drift from whatever
    // rows actually exist in SubscriptionPlans.
    [HttpGet("/api/platform/subscription-plans")]
    [Authorize(Roles = "SuperAdmin,PlatformSupport")]
    [Authorize(Policy = "Permission:platform.tenants.read")]
    public async Task<IActionResult> GetSubscriptionPlans(CancellationToken ct)
    {
        var result = await _mediator.Send(new GetSubscriptionPlansQuery(), ct);
        return StatusCode(result.StatusCode, result);
    }

    // Same absolute-route reasoning as GetSubscriptionPlans above: a plan's feature membership
    // is global catalog data, not scoped to any one tenant.
    [HttpGet("/api/platform/subscription-plans/{id}/features")]
    [Authorize(Roles = "SuperAdmin,PlatformSupport")]
    [Authorize(Policy = "Permission:platform.tenants.read")]
    public async Task<IActionResult> GetPlanFeatures([FromRoute] int id, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetPlanFeaturesQuery(id), ct);
        return StatusCode(result.StatusCode, result);
    }

    [HttpPut("/api/platform/subscription-plans/{id}/features")]
    [Authorize(Roles = "SuperAdmin")]
    [Authorize(Policy = "Permission:platform.tenants.manage")]
    public async Task<IActionResult> UpdatePlanFeatures(
        [FromRoute] int id,
        [FromBody] UpdatePlanFeaturesRequest request,
        CancellationToken ct)
    {
        var result = await _mediator.Send(new UpdatePlanFeaturesCommand(id, request.FeatureIds), ct);
        return StatusCode(result.StatusCode, result);
    }

    [HttpGet("{id}/features")]
    [Authorize(Roles = "SuperAdmin,PlatformSupport")]
    [Authorize(Policy = "Permission:platform.tenants.read")]
    public async Task<IActionResult> GetTenantFeatures(
        [FromRoute] Guid id,
        CancellationToken ct)
    {
        var result = await _mediator.Send(new GetTenantFeaturesQuery(id), ct);
        return StatusCode(result.StatusCode, result);
    }

    [HttpPatch("{id}/features")]
    [Authorize(Roles = "SuperAdmin")]
    [Authorize(Policy = "Permission:platform.tenants.manage")]
    public async Task<IActionResult> ToggleFeature(
        [FromRoute] Guid id,
        [FromBody] ToggleFeatureRequest request,
        CancellationToken ct)
    {
        var result = await _mediator.Send(
            new ToggleFeatureCommand(id, request.FeatureId, request.IsEnabled, request.Lock), ct);
        return StatusCode(result.StatusCode, result);
    }

    [HttpPatch("{id}/subscription")]
    [Authorize(Roles = "SuperAdmin")]
    [Authorize(Policy = "Permission:platform.tenants.manage")]
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
    [Authorize(Roles = "SuperAdmin")]
    [Authorize(Policy = "Permission:platform.tenants.manage")]
    public async Task<IActionResult> ExpireSubscription(
        [FromRoute] Guid id,
        CancellationToken ct)
    {
        var result = await _mediator.Send(
            new ExpireTenantSubscriptionCommand(id), ct);
        return StatusCode(result.StatusCode, result);
    }

    [HttpPost("{id}/subscription/trial")]
    [Authorize(Roles = "SuperAdmin")]
    [Authorize(Policy = "Permission:platform.tenants.manage")]
    public async Task<IActionResult> SetTrial(
        [FromRoute] Guid id,
        CancellationToken ct)
    {
        var result = await _mediator.Send(
            new SetTenantTrialCommand(id), ct);
        return StatusCode(result.StatusCode, result);
    }

    [HttpPost("{id}/impersonate")]
    [Authorize(Roles = "SuperAdmin")]
    [Authorize(Policy = "Permission:platform.impersonate")]
    public async Task<IActionResult> StartImpersonation(
        [FromRoute] Guid id,
        [FromBody] StartImpersonationRequest request,
        CancellationToken ct)
    {
        var result = await _mediator.Send(new StartImpersonationCommand(id, request.Reason), ct);
        return StatusCode(result.StatusCode, result);
    }
}

public record StartImpersonationRequest(string Reason);

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

public record ChangeTenantStatusRequest(TenantStatus NewStatus, string? Reason = null);

public record ArchiveTenantRequest(string Reason);

public record ChangeTenantPlanRequest(int NewPlanId);

public record UpdatePlanFeaturesRequest(List<Guid> FeatureIds);

// Lock: whether this decision should also stop the tenant from changing it back themselves
// (LockedBySuperAdmin) - the SuperAdmin decides this explicitly on every toggle rather than it
// always being forced, see ToggleFeatureCommandHandler.
public record ToggleFeatureRequest(Guid FeatureId, bool IsEnabled, bool Lock);

public record ExtendSubscriptionRequest(int Days);
