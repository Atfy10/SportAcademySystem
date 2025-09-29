using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SportAcademy.Application.Commands.Trainees.CreateTrainee;
using SportAcademy.Application.Commands.UserCommands.UserDelete;
using SportAcademy.Application.Commands.UserCommands.UserUpdate;
using SportAcademy.Application.Queries.UserQueries.GetById;

namespace SportAcademy.Web.Controllers
{
    //[Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : Controller
    {
        IMediator _mediator;

        public UserController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet("get-all")]
        public async Task<IActionResult> Index()
        {
            var users = await _mediator.Send(new object());
            return Ok(users);
        }

        [HttpGet("get/{id}")]
        public async Task<IActionResult> Details(string id)
        {
            var user = await _mediator.Send(new GetUserByIdQuery(id));
            return Ok(user);
        }

        [HttpPost("create")]
        public async Task<IActionResult> CreateAsync(CreateTraineeCommand command)
        {
            var user = await _mediator.Send(command);
            return Ok(user);
        }

        [HttpPut("update")]
        public async Task<IActionResult> EditAsync(UpdateUserCommand command)
        {
            var user = await _mediator.Send(command);
            return Ok(user);
        }

        [HttpDelete("delete")]
        public IActionResult Delete(DeleteUserCommand command)
        {
            var isDeleted = _mediator.Send(command);
            if (isDeleted is null || !isDeleted.Result.IsSuccess)
                return BadRequest(isDeleted?.Result.Message);

            return NoContent();
        }
    }
}
