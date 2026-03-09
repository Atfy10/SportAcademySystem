using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SportAcademy.Application.Commands.Trainees.CreateTrainee;
using SportAcademy.Application.Commands.UserCommands.UserCreate;
using SportAcademy.Application.Commands.UserCommands.UserDelete;
using SportAcademy.Application.Commands.UserCommands.UserUpdate;
using SportAcademy.Application.Queries.TraineeQueries.GetAll;
using SportAcademy.Application.Queries.UserQueries.GetAll;
using SportAcademy.Application.Queries.UserQueries.GetById;
using SportAcademy.Application.Queries.UserQueries.GetUnlinkedUsers;

namespace SportAcademy.Web.Controllers
{
    //[Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        IMediator _mediator;

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
            var user = await _mediator.Send(new GetUserByIdQuery(id));
            return Ok(user);
        }

        [HttpPost]
        public async Task<IActionResult> CreateAsync(CreateUserCommand command)
        {
            var user = await _mediator.Send(command);
            return Ok(user);
        }

        [HttpPut]
        public async Task<IActionResult> EditAsync(UpdateUserCommand command)
        {
            var user = await _mediator.Send(command);
            return Ok(user);
        }

        [HttpDelete]
        public IActionResult Delete(DeleteUserCommand command)
        {
            var isDeleted = _mediator.Send(command);
            if (isDeleted is null || !isDeleted.Result.IsSuccess)
                return BadRequest(isDeleted?.Result.Message);

            return NoContent();
        }
    }
}
