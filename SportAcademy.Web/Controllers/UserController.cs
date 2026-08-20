using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SportAcademy.Application.Commands.UserCommands.UpdateUserPermissions;
using SportAcademy.Application.Commands.UserCommands.UserCreate;
using SportAcademy.Application.Commands.UserCommands.UserDelete;
using SportAcademy.Application.Commands.UserCommands.UserUpdate;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.Queries.TraineeQueries.GetAll;
using SportAcademy.Application.Queries.UserQueries.GetAll;
using SportAcademy.Application.Queries.UserQueries.GetById;
using SportAcademy.Application.Queries.UserQueries.GetMeQuery;
using SportAcademy.Application.Queries.UserQueries.GetUnlinkedUsers;

namespace SportAcademy.Web.Controllers
{
[Authorize]
[EnableRateLimiting("per-user")]
[ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly IMediator _mediator;

        public UserController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var users = await _mediator.Send(new GetAllUsersQuery());
            return Ok(users);
        }

        [HttpGet("unlinked")]
        public async Task<IActionResult> GetUnlinked()
        {
            var users = await _mediator.Send(new GetUnlinkedUsersQuery());
            return Ok(users);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Details(string id)
        {
            var user = await _mediator.Send(new GetUserByIdQuery(Guid.Parse(id)));
            return Ok(user);
        }

        // Mutating actions require the tenant.users.manage permission rather than a hardcoded
        // role list - that permission is granted to Admin/Owner by default, but can also be
        // handed to an individual user (see UpdateUserPermissionsCommand) without promoting
        // them to Admin.
        [Authorize(Policy = "Permission:tenant.users.manage")]
        [HttpPost]
        [ProducesResponseType(typeof(Result<string>), StatusCodes.Status200OK)]
        public async Task<IActionResult> CreateAsync(CreateUserCommand command)
        {
            var user = await _mediator.Send(command);
            return Ok(user);
        }

        [Authorize(Policy = "Permission:tenant.users.manage")]
        [HttpPut]
        public async Task<IActionResult> EditAsync(UpdateUserCommand command)
        {
            var user = await _mediator.Send(command);
            return Ok(user);
        }

        [Authorize(Policy = "Permission:tenant.users.manage")]
        [HttpDelete]
        public async Task<IActionResult> Delete(DeleteUserCommand command, CancellationToken ct)
        {
            var result = await _mediator.Send(command, ct);
            if (!result.IsSuccess)
                return BadRequest(result.Message);

            return NoContent();
        }

        [Authorize(Policy = "Permission:tenant.users.manage")]
        [HttpPut("{id}/permissions")]
        public async Task<IActionResult> UpdatePermissions(
            Guid id, [FromBody] List<string> permissions, CancellationToken ct)
        {
            var result = await _mediator.Send(new UpdateUserPermissionsCommand(id, permissions), ct);
            if (!result.IsSuccess)
                return BadRequest(result);

            return Ok(result);
        }

        [HttpGet("me")]
        public async Task<IActionResult> GetMe(CancellationToken ct)
        {
            var result = await _mediator.Send(new GetMeQuery(), ct);
            return Ok(result);
        }
    }
}
