using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SportAcademy.Application.Commands.AttendanceCommands.CreateAttendance;
using SportAcademy.Application.Commands.AttendanceCommands.DeleteAttendance;
using SportAcademy.Application.Commands.AttendanceCommands.UpdateAttendance;
using SportAcademy.Application.Commands.TraineeGroupCommands.CreateTraineeGroup;
using SportAcademy.Application.Commands.TraineeGroupCommands.DeleteTraineeGroup;
using SportAcademy.Application.Commands.TraineeGroupCommands.UpdateTraineeGroup;
using SportAcademy.Application.Common.Pagination;
using SportAcademy.Application.Queries.AttendanceQueries.GetById;
using SportAcademy.Application.Queries.BranchQueries.GetAll;
using SportAcademy.Application.Queries.TraineeGroupQueries.GetAll;
using SportAcademy.Application.Queries.TraineeGroupQueries.GetAllOfSpecificDay;
using SportAcademy.Application.Queries.TraineeGroupQueries.GetById;

namespace SportAcademy.Web.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class TraineeGroupController : ControllerBase
    {
        private readonly IMediator _mediator;

        public TraineeGroupController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost("create")]
        public async Task<IActionResult> Create([FromBody] CreateTraineeGroupCommand command, CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(command, cancellationToken);
            return Ok(result);
        }

        [HttpGet]
        public async Task<IActionResult> GetAll(
            [FromQuery] int? page,
            [FromQuery] int? pageSize,
            CancellationToken cancellationToken
            )
        {
            var result = await _mediator.Send(
                new GetAllTraineeGroupsQuery(PageRequest.Create(page, pageSize)),
                cancellationToken);
            return Ok(result);
        }

        [HttpGet("get/{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _mediator.Send(new GetTraineeGroupByIdQuery(id));
            return Ok(result);
        }

        [HttpPut("update")]
        public async Task<IActionResult> Update([FromBody] UpdateTraineeGroupCommand command,
            CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(command, cancellationToken);
            return Ok(result);
        }

        [HttpDelete("delete")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _mediator.Send(new DeleteTraineeGroupCommand(id));
            return Ok(result);
        }

        [HttpGet("get-all-for-specific-day")]
        public async Task<IActionResult> GetAllForDay(
            [FromQuery] DateTime date,
            [FromQuery] int? page,
            [FromQuery] int? pageSize,
            CancellationToken ct)
        {
            var result = await _mediator.Send(new GetAllSessionsOfSpecificDayQuery(
                                            date,
                                            PageRequest.Create(page, pageSize)), ct);
            return Ok(result);
        }
    }
}
