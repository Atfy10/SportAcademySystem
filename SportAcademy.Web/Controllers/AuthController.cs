using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SportAcademy.Application.Commands.AuthCommands.AdminCreateUser;
using SportAcademy.Application.Commands.AuthCommands.AdminResetUserPassword;
using SportAcademy.Application.Commands.AuthCommands.AssignRolesToUser;
using SportAcademy.Application.Commands.AuthCommands.ChangePassword;
using SportAcademy.Application.Commands.AuthCommands.Login;
using SportAcademy.Application.Commands.AuthCommands.RefreshToken;
using SportAcademy.Application.Commands.AuthCommands.ResetPassword;
using SportAcademy.Application.Commands.AuthCommands.RevokeToken;
using SportAcademy.Application.Commands.AuthCommands.ToggleUserActive;
using SportAcademy.Application.Commands.AuthCommands.UpdateUserBranches;
using SportAcademy.Application.Commands.AuthCommands.VerifyPassword;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.DTOs.AuthDtos;
using SportAcademy.Application.Queries.AuthQueries.GetAllRoles;
using SportAcademy.Application.Queries.AuthQueries.GetMyPermissions;
using SportAcademy.Application.Queries.AuthQueries.GetUserBranches;

namespace SportAcademy.Web.Controllers
{
[ApiController]
[EnableRateLimiting("public")]
[Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IMediator _mediator;
        public AuthController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginCommand command, CancellationToken ct)
        {
            var result = await _mediator.Send(command, ct);
            return StatusCode(result.StatusCode, result);
        }

        // Public: consumes the token from a "send reset link" email (see
        // SendOwnerPasswordResetLinkCommand / Platform/OwnersController). No [Authorize] here,
        // same as Login above - the whole point is the caller isn't signed in yet.
        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword(ResetPasswordCommand command, CancellationToken ct)
        {
            var result = await _mediator.Send(command, ct);
            return StatusCode(result.StatusCode, result);
        }

        [Authorize(Policy = "Permission:tenant.users.manage")]
        [HttpGet("roles")]
        public async Task<IActionResult> GetAllRoles(CancellationToken ct)
        {
            var result = await _mediator.Send(new GetAllRolesQuery(), ct);
            return StatusCode(result.StatusCode, result);
        }

        [Authorize(Policy = "Permission:tenant.users.manage")]
        [HttpGet("permissions")]
        public IActionResult GetAllPermissions()
        {
            return Ok(Result<IReadOnlyList<string>>.Success(
                SportAcademy.Domain.Authorization.Permissions.All.Where(p => !p.StartsWith("platform.")).ToList(),
                "GetAllPermissions"));
        }

        [Authorize(Policy = "Permission:tenant.users.manage")]
        [HttpPost("users")]
        public async Task<IActionResult> AdminCreateUser([FromBody] AdminCreateUserCommand command, CancellationToken ct)
        {
            var result = await _mediator.Send(command, ct);
            return StatusCode(result.StatusCode, result);
        }

        [Authorize(Policy = "Permission:tenant.users.manage")]
        [HttpPost("users/{userId}/toggle-active")]
        public async Task<IActionResult> ToggleUserActive([FromRoute] string userId, CancellationToken ct)
        {
            var result = await _mediator.Send(new ToggleUserActiveCommand(Guid.Parse(userId)), ct);
            return StatusCode(result.StatusCode, result);
        }

        [Authorize(Policy = "Permission:tenant.users.manage")]
        [HttpPost("users/{userId}/roles")]
        public async Task<IActionResult> AssignRoles([FromRoute] string userId, [FromBody] List<string> roles, CancellationToken ct)
        {
            var result = await _mediator.Send(new AssignRolesToUserCommand(Guid.Parse(userId), roles), ct);
            return StatusCode(result.StatusCode, result);
        }

        [Authorize(Policy = "Permission:tenant.users.manage")]
        [HttpGet("users/{userId}/branches")]
        public async Task<IActionResult> GetUserBranches([FromRoute] string userId, CancellationToken ct)
        {
            var result = await _mediator.Send(new GetUserBranchesQuery(Guid.Parse(userId)), ct);
            return StatusCode(result.StatusCode, result);
        }

        [Authorize(Policy = "Permission:tenant.users.manage")]
        [HttpPut("users/{userId}/branches")]
        public async Task<IActionResult> UpdateUserBranches(
            [FromRoute] string userId, [FromBody] List<int> branchIds, CancellationToken ct)
        {
            var result = await _mediator.Send(new UpdateUserBranchesCommand(Guid.Parse(userId), branchIds), ct);
            return StatusCode(result.StatusCode, result);
        }

        [Authorize(Policy = "Permission:tenant.users.manage")]
        [HttpPost("users/{userId}/reset-password")]
        public async Task<IActionResult> AdminResetUserPassword(
            [FromRoute] string userId,
            [FromBody] AdminResetUserPasswordRequest request,
            CancellationToken ct)
        {
            var cmd = new AdminResetUserPasswordCommand(Guid.Parse(userId), request.AdminPassword, request.NewPassword);
            var result = await _mediator.Send(cmd, ct);
            return StatusCode(result.StatusCode, result);
        }

        // Fresh, server-resolved roles/permissions for the caller - the frontend polls this
        // instead of trusting the access token's "permission" claims, which can be up to
        // Jwt:ExpireMinutes stale and would otherwise make an admin's Deny invisible in the UI
        // until the caller's token happens to refresh.
        [Authorize]
        [HttpGet("me/permissions")]
        public async Task<IActionResult> GetMyPermissions(CancellationToken ct)
        {
            var result = await _mediator.Send(new GetMyPermissionsQuery(), ct);
            return StatusCode(result.StatusCode, result);
        }

        [Authorize]
        [HttpPost("change-password")]
        public async Task<IActionResult> ChangePassword(ChangePasswordCommand command, CancellationToken ct)
        {
            var result = await _mediator.Send(command, ct);
            return StatusCode(result.StatusCode, result);
        }

        [Authorize]
        [HttpPost("verify-password")]
        public async Task<IActionResult> VerifyPassword([FromBody] VerifyPasswordCommand command, CancellationToken ct)
        {
            var result = await _mediator.Send(command, ct);
            return StatusCode(result.StatusCode, result);
        }

        [AllowAnonymous]
        [HttpPost("refresh")]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest request, CancellationToken ct)
        {
            var result = await _mediator.Send(new RefreshTokenCommand(request.RefreshToken), ct);
            return StatusCode(result.StatusCode, result);
        }

        [AllowAnonymous]
        [EnableRateLimiting("token-revoke")]
        [HttpPost("revoke")]
        public async Task<IActionResult> RevokeToken([FromBody] RefreshTokenRequest request, CancellationToken ct)
        {
            var result = await _mediator.Send(new RevokeTokenCommand(request.RefreshToken), ct);
            return StatusCode(result.StatusCode, result);
        }
    }
}
