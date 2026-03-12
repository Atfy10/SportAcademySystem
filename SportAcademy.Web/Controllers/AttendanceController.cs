using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SportAcademy.Application.Commands.AttendanceCommands.CreateAttendance;
using SportAcademy.Application.Commands.AttendanceCommands.DeleteAttendance;
using SportAcademy.Application.Commands.AttendanceCommands.UpdateAttendance;
using SportAcademy.Application.Queries.AttendanceQueries.GetAttendanceRate;
using SportAcademy.Application.Queries.AttendanceQueries.GetById;
using SportAcademy.Application.Queries.AttendanceQueries.GetGlobalAttendanceRate;
using SportAcademy.Application.Queries.BranchQueries.GetAll;
using SportAcademy.Domain.Enums;

namespace SportAcademy.Web.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class AttendanceController : ControllerBase
    {
        private readonly IMediator _mediator;

        public AttendanceController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateAttendanceCommand command, CancellationToken ct)
        {
            var result = await _mediator.Send(command, ct);
            return Ok(result);
        }

        [HttpGet]
        public async Task<IActionResult> GetAll(CancellationToken ct)
        {
            var result = await _mediator.Send(new GetAllAttendancesQuery(),ct);
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id, CancellationToken ct)
        {
            var result = await _mediator.Send(new GetAttendanceByIdQuery(id), ct);
            return Ok(result);
        }

        [HttpPut]
        public async Task<IActionResult> Update([FromBody] UpdateAttendanceCommand command,
            CancellationToken ct)
        {
            var result = await _mediator.Send(command, ct);
            return Ok(result);
        }

        [HttpDelete]
        public async Task<IActionResult> Delete(int id, CancellationToken ct)
        {
            var result = await _mediator.Send(new DeleteAttendanceCommand(id), ct);
            return Ok(result);
        }

        [HttpGet("trainee/{traineeId}/rate")]
        public async Task<IActionResult> GetAttendanceRate(
            [FromRoute] int traineeId,
            [FromQuery] DateOnly? from,
            [FromQuery] DateOnly? to,
            CancellationToken ct)
        {
            var result = await _mediator.Send(new GetAttendanceRateQuery(
                    traineeId, from, to
                ), ct);
            return Ok(result);
        }

        [HttpGet("rate")]
        public async Task<IActionResult> GetAttendanceRate(
            [FromQuery] Month? month,
            CancellationToken ct)
        {
            var result = await _mediator.Send(new GetGlobalAttendanceRateQuery(month), ct);
            return Ok(result);
        }
    }
}
