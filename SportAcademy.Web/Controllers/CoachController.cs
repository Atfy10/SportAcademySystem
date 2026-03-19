using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SportAcademy.Application.Commands.CoachCommands.CreateCoach;
using SportAcademy.Application.Commands.CoachCommands.CreateCoachWithEmployee;
using SportAcademy.Application.Commands.CoachCommands.DeleteCoach;
using SportAcademy.Application.Common.Pagination;
using SportAcademy.Application.Queries.CoachQueries.GetAllForDropdown;
using SportAcademy.Application.Queries.CoachQueries.GetAverageRating;
using SportAcademy.Application.Queries.CoachQueries.GetById;
using SportAcademy.Application.Queries.CoachQueries.GetCoachsCount;
using SportAcademy.Application.Queries.CoachQueries.SearchCoachs;

namespace SportAcademy.Web.Controllers
{
    [Authorize]
    [ApiController]
    public class CoachController : ControllerBase
    {
        private readonly IMediator _mediator;

        public CoachController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet("api/coach/{id}")]
        public async Task<ActionResult> GetById(int id, CancellationToken ct)
        {
            var result = await _mediator.Send(new GetCoachByIdQuery(id), ct);
            return Ok(result);
        }

        [HttpGet("api/coach/dropdown")]
        public async Task<IActionResult> GetDropdown(CancellationToken ct)
        {
            var result = await _mediator.Send(new GetAllCoachesForDropdownQuery(), ct);
            return Ok(result);
        }

        [HttpPost("api/coach")]
        public async Task<ActionResult> Create(CreateCoachCommand command, CancellationToken ct)
        {
            var result = await _mediator.Send(command, ct);
            return Ok(result);
        }

        [HttpPost("api/coach/with-employee")]
        public async Task<ActionResult> CreateFromEmployee(CreateCoachWithEmployeeCommand command, CancellationToken ct)
        {
            var result = await _mediator.Send(command, ct);
            return Ok(result);
        }

        [HttpDelete("api/coaches/{id}")]
        public async Task<ActionResult> Delete(int id, CancellationToken ct)
        {
            var result = await _mediator.Send(new DeleteCoachCommand(id), ct);
            return Ok(result);
        }

        [HttpGet("api/coach/rating-average")]
        public async Task<IActionResult> GetAllCoachsAvgRating()
        {
            var result = await _mediator.Send(new GetAverageRatingQuery());
            return Ok(result);
        }

        [HttpGet("api/coach/count")]
        public async Task<IActionResult> GetAllCoachsCount()
        {
            var result = await _mediator.Send(new GetCoachsCountQuery());
            return Ok(result);
        }

        [HttpGet("api/coach/search")]
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
