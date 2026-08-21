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
using SportAcademy.Application.Commands.AuthCommands.VerifyPassword;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.DTOs.AuthDtos;
using SportAcademy.Application.Queries.AuthQueries.GetAllRoles;
using SportAcademy.Application.Queries.AuthQueries.GetMyProfile;

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
            return Ok(result);
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
            return Ok(result);
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
            return Ok(result);
        }

        [Authorize(Policy = "Permission:tenant.users.manage")]
        [HttpPost("users/{userId}/toggle-active")]
        public async Task<IActionResult> ToggleUserActive([FromRoute] string userId, CancellationToken ct)
        {
            var result = await _mediator.Send(new ToggleUserActiveCommand(Guid.Parse(userId)), ct);
            return Ok(result);
        }

        [Authorize(Policy = "Permission:tenant.users.manage")]
        [HttpPost("users/{userId}/roles")]
        public async Task<IActionResult> AssignRoles([FromRoute] string userId, [FromBody] List<string> roles, CancellationToken ct)
        {
            var result = await _mediator.Send(new AssignRolesToUserCommand(Guid.Parse(userId), roles), ct);
            return Ok(result);
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
            return Ok(result);
        }

        //[Authorize]
        //[HttpGet("~/api/user/me")]
        //public async Task<IActionResult> GetMyProfile(CancellationToken ct)
        //{
        //    var result = await _mediator.Send(new GetMyProfileQuery(), ct);
        //    return Ok(result);
        //}

        [Authorize]
        [HttpPost("change-password")]
        public async Task<IActionResult> ChangePassword(ChangePasswordCommand command, CancellationToken ct)
        {
            var result = await _mediator.Send(command, ct);
            return Ok(result);
        }

        [Authorize]
        [HttpPost("verify-password")]
        public async Task<IActionResult> VerifyPassword([FromBody] VerifyPasswordCommand command, CancellationToken ct)
        {
            var result = await _mediator.Send(command, ct);
            return Ok(result);
        }

        [AllowAnonymous]
        [HttpPost("refresh")]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest request, CancellationToken ct)
        {
            var result = await _mediator.Send(new RefreshTokenCommand(request.RefreshToken), ct);
            return Ok(result);
        }

        [AllowAnonymous]
        [EnableRateLimiting("token-revoke")]
        [HttpPost("revoke")]
        public async Task<IActionResult> RevokeToken([FromBody] RefreshTokenRequest request, CancellationToken ct)
        {
            var result = await _mediator.Send(new RevokeTokenCommand(request.RefreshToken), ct);
            return Ok(result);
        }
    }
}
