using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SportAcademy.Application.Commands.CoachCommands.CreateCoach;
using SportAcademy.Application.Commands.CoachCommands.CreateCoachWithEmployee;
using SportAcademy.Application.Commands.CoachCommands.DeleteCoach;

namespace SportAcademy.Web.Controllers
{
    //[Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class CoachController : ControllerBase
    {
        private readonly IMediator _mediator;

        public CoachController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost("create")]
        public async Task<ActionResult> Create(CreateCoachCommand command, CancellationToken ct)
        {
            var result = await _mediator.Send(command, ct);
            return Ok(result);
        }

        [HttpPost("create-with-employee")]
        public async Task<ActionResult> CreateWithEmployee(CreateCoachWithEmployeeCommand command, CancellationToken ct)
        {
            var result = await _mediator.Send(command, ct);
            return Ok(result);
        }

        [HttpDelete("delete")]
        public async Task<ActionResult> Delete(int employeeId, CancellationToken ct)
        {
            var result = await _mediator.Send(new DeleteCoachCommand(employeeId), ct);
            return Ok(result);
        }
    }
}
