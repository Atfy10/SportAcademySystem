using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SportAcademy.Application.Commands.AuthCommands.AcceptInvitation;
using SportAcademy.Application.Commands.AuthCommands.CreateInvitation;
using SportAcademy.Application.Commands.AuthCommands.ResendInvitation;
using SportAcademy.Application.Queries.AuthQueries.ValidateInvitation;

namespace SportAcademy.Web.Controllers
{
[ApiController]
[EnableRateLimiting("public")]
public class OnboardingController : ControllerBase
    {
        private readonly IMediator _mediator;

        public OnboardingController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [Authorize(Policy = "Permission:tenant.users.manage")]
        [HttpPost("/api/tenants/{tenantId}/invitations")]
        public async Task<IActionResult> CreateInvitation(
            [FromRoute] Guid tenantId,
            [FromBody] CreateInvitationRequest request,
            CancellationToken ct)
        {
            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (userIdClaim is null || !Guid.TryParse(userIdClaim, out var userId))
                return Unauthorized();

            if (!CallerCanManageTenant(tenantId))
                return Forbid();

            var command = new CreateInvitationCommand(
                tenantId, request.Email, userId, request.Role, request.Permissions, request.ExpiresAt);

            var result = await _mediator.Send(command, ct);
            return Ok(result);
        }

        [Authorize(Policy = "Permission:tenant.users.manage")]
        [HttpPost("/api/tenants/{tenantId}/invitations/resend")]
        public async Task<IActionResult> ResendInvitation(
            [FromRoute] Guid tenantId,
            [FromBody] CreateInvitationRequest request,
            CancellationToken ct)
        {
            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (userIdClaim is null || !Guid.TryParse(userIdClaim, out var userId))
                return Unauthorized();

            if (!CallerCanManageTenant(tenantId))
                return Forbid();

            var command = new ResendInvitationCommand(tenantId, request.Email, userId);

            var result = await _mediator.Send(command, ct);
            return Ok(result);
        }

        // SuperAdmin manages invitations across tenants (e.g. inviting a brand-new tenant's
        // first Owner) via the Platform console; every other caller may only invite into
        // their own tenant, even though they hold the tenant.users.manage permission -
        // without this, the route's tenantId was previously trusted with no ownership check
        // at all, letting any authenticated user invite into any tenant by editing the URL.
        private bool CallerCanManageTenant(Guid tenantId)
        {
            if (User.IsInRole("SuperAdmin")) return true;

            var callerTenantId = User.FindFirst("tenant_id")?.Value;
            return Guid.TryParse(callerTenantId, out var parsed) && parsed == tenantId;
        }

        [AllowAnonymous]
        [HttpGet("/api/t/{slug}/invite/{token}")]
        public async Task<IActionResult> ValidateInvitation(
            [FromRoute] string slug,
            [FromRoute] string token,
            CancellationToken ct)
        {
            var result = await _mediator.Send(new ValidateInvitationQuery(token), ct);
            return Ok(result);
        }

        [AllowAnonymous]
        [HttpPost("/api/t/{slug}/invite/{token}/accept")]
        public async Task<IActionResult> AcceptInvitation(
            [FromRoute] string slug,
            [FromRoute] string token,
            [FromBody] AcceptInvitationRequest request,
            CancellationToken ct)
        {
            var command = new AcceptInvitationCommand(token, request.Password, slug);
            var result = await _mediator.Send(command, ct);
            return Ok(result);
        }
    }

    public record CreateInvitationRequest(
        string Email, string? Role = null, List<string>? Permissions = null, DateTime? ExpiresAt = null);

    public record AcceptInvitationRequest(string Password);
}
