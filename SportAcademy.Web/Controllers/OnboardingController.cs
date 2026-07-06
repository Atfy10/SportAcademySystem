using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SportAcademy.Application.Commands.AuthCommands.AcceptInvitation;
using SportAcademy.Application.Commands.AuthCommands.CreateInvitation;
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

        [Authorize]
        [HttpPost("/api/tenants/{tenantId}/invitations")]
        public async Task<IActionResult> CreateInvitation(
            [FromRoute] Guid tenantId,
            [FromBody] CreateInvitationRequest request,
            CancellationToken ct)
        {
            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (userIdClaim is null || !Guid.TryParse(userIdClaim, out var userId))
                return Unauthorized();

            var command = new CreateInvitationCommand(
                tenantId, request.Email, userId, request.ExpiresAt);

            var result = await _mediator.Send(command, ct);
            return Ok(result);
        }

        [AllowAnonymous]
        [HttpGet("/t/{slug}/invite/{token}")]
        public async Task<IActionResult> ValidateInvitation(
            [FromRoute] string slug,
            [FromRoute] string token,
            CancellationToken ct)
        {
            var result = await _mediator.Send(new ValidateInvitationQuery(token), ct);
            return Ok(result);
        }

        [AllowAnonymous]
        [HttpPost("/t/{slug}/invite/{token}/accept")]
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

    public record CreateInvitationRequest(string Email, DateTime? ExpiresAt);

    public record AcceptInvitationRequest(string Password);
}
