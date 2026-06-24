using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SportAcademy.Application.Commands.AuthCommands.AdminCreateUser;
using SportAcademy.Application.Commands.AuthCommands.AdminResetUserPassword;
using SportAcademy.Application.Commands.AuthCommands.AssignRolesToUser;
using SportAcademy.Application.Commands.AuthCommands.ChangePassword;
using SportAcademy.Application.Commands.AuthCommands.Login;
using SportAcademy.Application.Commands.AuthCommands.RefreshToken;
using SportAcademy.Application.Commands.AuthCommands.Register;
using SportAcademy.Application.Commands.AuthCommands.RevokeToken;
using SportAcademy.Application.Commands.AuthCommands.ToggleUserActive;
using SportAcademy.Application.Commands.AuthCommands.VerifyPassword;
using SportAcademy.Application.DTOs.AuthDtos;
using SportAcademy.Application.Queries.AuthQueries.GetAllRoles;
using SportAcademy.Application.Queries.AuthQueries.GetMyProfile;

namespace SportAcademy.Web.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IMediator _mediator;
        public AuthController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost("sign-up")]
        public async Task<IActionResult> SignUp(RegisterCommand command, CancellationToken ct)
        {
            var result = await _mediator.Send(command, ct);
            return Ok(result);
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginCommand command, CancellationToken ct)
        {
            var result = await _mediator.Send(command, ct);
            return Ok(result);
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("roles")]
        public async Task<IActionResult> GetAllRoles(CancellationToken ct)
        {
            var result = await _mediator.Send(new GetAllRolesQuery(), ct);
            return Ok(result);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost("users")]
        public async Task<IActionResult> AdminCreateUser([FromBody] AdminCreateUserCommand command, CancellationToken ct)
        {
            var result = await _mediator.Send(command, ct);
            return Ok(result);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost("users/{userId}/toggle-active")]
        public async Task<IActionResult> ToggleUserActive([FromRoute] string userId, CancellationToken ct)
        {
            var result = await _mediator.Send(new ToggleUserActiveCommand(Guid.Parse(userId)), ct);
            return Ok(result);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost("users/{userId}/roles")]
        public async Task<IActionResult> AssignRoles([FromRoute] string userId, [FromBody] List<string> roles, CancellationToken ct)
        {
            var result = await _mediator.Send(new AssignRolesToUserCommand(Guid.Parse(userId), roles), ct);
            return Ok(result);
        }

        [Authorize(Roles = "Admin")]
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

        [Authorize]
        [HttpGet("~/api/user/me")]
        public async Task<IActionResult> GetMyProfile(CancellationToken ct)
        {
            var result = await _mediator.Send(new GetMyProfileQuery(), ct);
            return Ok(result);
        }

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

        [Authorize]
        [HttpPost("revoke")]
        public async Task<IActionResult> RevokeToken([FromBody] RefreshTokenRequest request, CancellationToken ct)
        {
            var result = await _mediator.Send(new RevokeTokenCommand(request.RefreshToken), ct);
            return Ok(result);
        }
    }
}
