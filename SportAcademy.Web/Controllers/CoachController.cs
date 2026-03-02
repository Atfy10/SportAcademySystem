using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SportAcademy.Application.Commands.CoachCommands.CreateCoach;
using SportAcademy.Application.Commands.CoachCommands.CreateCoachWithEmployee;
using SportAcademy.Application.Commands.CoachCommands.DeleteCoach;
using SportAcademy.Application.Common.Pagination;
using SportAcademy.Application.Queries.CoachQueries.GetAverageRating;
using SportAcademy.Application.Queries.CoachQueries.GetCoachsCount;
using SportAcademy.Application.Queries.CoachQueries.SearchCoachs;
using SportAcademy.Application.Queries.EmployeeQueries.GetById;

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

        [HttpGet("{id}")]
        public async Task<ActionResult> Details(int id, CancellationToken ct)
        {
            var result = await _mediator.Send(new GetCoachByIdQuery(id), ct);
            return Ok(result);
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

        [HttpGet("rating-average")]
        public async Task<IActionResult> GetAllCoachsAvgRating()
        {
            var result = await _mediator.Send(new GetAverageRatingQuery());
            return Ok(result);
        }

        [HttpGet("count")]
        public async Task<IActionResult> GetAllCoachsCount()
        {
            var result = await _mediator.Send(new GetCoachsCountQuery());
            return Ok(result);
        }

        [HttpGet("search")]
        public async Task<IActionResult> SearchCoaches(
            [FromQuery] string searchTerm,
            [FromQuery] int? page,
            [FromQuery] int? pageSize,
            CancellationToken ct)
        {
            var result = await _mediator.Send(new SearchCoachQuery(
                                        searchTerm, PageRequest.Create(page, pageSize)), ct);
            return Ok(result);
        }
    }
}
