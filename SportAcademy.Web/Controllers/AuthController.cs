using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SportAcademy.Application.Commands.AuthCommands.ChangePassword;
using SportAcademy.Application.Commands.AuthCommands.Login;
using SportAcademy.Application.Commands.AuthCommands.RefreshToken;
using SportAcademy.Application.Commands.AuthCommands.Register;
using SportAcademy.Application.Commands.AuthCommands.RevokeToken;
using SportAcademy.Application.Commands.AuthCommands.ToggleUserActive;
using SportAcademy.Application.Commands.UserCommands.UserCreate;
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

        [Authorize]
        [HttpGet("roles")]
        public async Task<IActionResult> GetAllRoles(CancellationToken ct)
        {
            var result = await _mediator.Send(new GetAllRolesQuery(), ct);
            return Ok(result);
        }

        [Authorize]
        [HttpGet("users")]
        public async Task<IActionResult> CreateUser(CreateUserCommand command, CancellationToken ct)
        {
            var result = await _mediator.Send(command, ct);
            return Ok(result);
        }

        [Authorize]
        [HttpPost("users/{userId}/toggle-active")]
        public async Task<IActionResult> ToggleUserActive([FromRoute] string userId, CancellationToken ct)
        {
            var result = await _mediator.Send(new ToggleUserActiveCommand(userId), ct);
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
