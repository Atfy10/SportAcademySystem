using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SportAcademy.Application.Commands.CoachCommands.CreateCoach;
using SportAcademy.Application.Commands.CoachCommands.CreateCoachWithEmployee;
using SportAcademy.Application.Commands.CoachCommands.DeleteCoach;
using SportAcademy.Application.Commands.CoachCommands.RateCoach;
using SportAcademy.Application.Commands.CoachCommands.UpdateCoach;
using SportAcademy.Application.Common.Localization;
using SportAcademy.Application.Common.Pagination;
using SportAcademy.Application.Queries.CoachQueries.GetAllForDropdown;
using SportAcademy.Application.Queries.CoachQueries.GetAverageRating;
using SportAcademy.Application.Queries.CoachQueries.GetById;
using SportAcademy.Application.Queries.CoachQueries.GetCoachsCount;
using SportAcademy.Application.Queries.CoachQueries.SearchCoachs;
using SportAcademy.Application.Common.Result;
using SportAcademy.Domain.Enums;

namespace SportAcademy.Web.Controllers
{
[Authorize]
[EnableRateLimiting("per-user")]
[Route("api/[controller]")]
    [ApiController]
    public class CoachController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly ILocalizationService _localizer;

        public CoachController(IMediator mediator, ILocalizationService localizer)
        {
            _mediator = mediator;
            _localizer = localizer;
        }

        [HttpGet("{id}")]
        [Authorize(Policy = "Permission:coach.manage")]
        public async Task<ActionResult> GetById(int id, CancellationToken ct)
        {
            var result = await _mediator.Send(new GetCoachByIdQuery(id), ct);
            return Ok(result);
        }

        [HttpGet("dropdown")]
        public async Task<IActionResult> GetDropdown(CancellationToken ct)
        {
            var result = await _mediator.Send(new GetAllCoachesForDropdownQuery(), ct);
            return Ok(result);
        }

        [HttpPost]
        [Authorize(Policy = "Permission:coach.manage")]
        public async Task<ActionResult> Create(CreateCoachCommand command, CancellationToken ct)
        {
            var result = await _mediator.Send(command, ct);
            return Ok(result);
        }

        [HttpPut("{id}")]
        [Authorize(Policy = "Permission:coach.manage")]
        public async Task<ActionResult> Update(int id, UpdateCoachCommand command, CancellationToken ct)
        {
            var cmd = command with { Id = id };
            var result = await _mediator.Send(cmd, ct);
            return Ok(result);
        }

        // Backend-only for now: adjusts a coach's star rating (1-5) after the initial
        // Rate = 3 default set at creation. No frontend UI calls this yet.
        [HttpPatch("{id}/rate")]
        [Authorize(Policy = "Permission:coach.manage")]
        public async Task<ActionResult> Rate(int id, [FromBody] int rate, CancellationToken ct)
        {
            var result = await _mediator.Send(new RateCoachCommand(id, rate), ct);
            return Ok(result);
        }

        [HttpPost("with-employee")]
        [Authorize(Policy = "Permission:coach.manage")]
        public async Task<ActionResult> CreateFromEmployee(CreateCoachWithEmployeeCommand command, CancellationToken ct)
        {
            var result = await _mediator.Send(command, ct);
            return Ok(result);
        }

        [HttpDelete("{id}")]
        [Authorize(Policy = "Permission:coach.manage")]
        public async Task<ActionResult> Delete(int id, CancellationToken ct)
        {
            var result = await _mediator.Send(new DeleteCoachCommand(id), ct);
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

        [HttpGet("skill-levels")]
        public IActionResult GetSkillLevels()
        {
            // value stays the English token the DB and raw SQL match on; only label is localized.
            var options = _localizer.Options<SkillLevel>(s => s != SkillLevel.NotSpecified);
            return Ok(Result<object>.Success(options, "GetSkillLevels"));
        }

        [HttpGet("search")]
        [Authorize(Policy = "Permission:coach.manage")]
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
