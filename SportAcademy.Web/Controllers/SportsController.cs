using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SportAcademy.Application.Commands.SportCommands.AddSkillLevel;
using SportAcademy.Application.Commands.SportCommands.CreateSport;
using SportAcademy.Application.Commands.SportCommands.DeleteSport;
using SportAcademy.Application.Commands.SportCommands.UpdateSport;
using SportAcademy.Application.Common.Pagination;
using SportAcademy.Application.Queries.CoachQueries.GetCoachsCount;
using SportAcademy.Application.Queries.SportQueries.GetAll;
using SportAcademy.Application.Queries.SportQueries.GetAvailableSportsForBranch;
using SportAcademy.Application.Queries.SportQueries.GetById;
using SportAcademy.Application.Queries.SportQueries.GetSportsCount;
using SportAcademy.Application.Queries.SportQueries.SearchSports;
using SportAcademy.Application.Queries.SportQueries.SearchSportsName;

namespace SportAcademy.Web.Controllers
{
    [Authorize]
    [ApiController]
    public class SportsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public SportsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost("api/sports")]
        public async Task<IActionResult> Create(CreateSportCommand command)
        {
            var result = await _mediator.Send(command);
            return Ok(result);
        }

        [HttpPut("api/sports")]
        public async Task<IActionResult> Update(UpdateSportCommand command)
        {
            var result = await _mediator.Send(command);
            return Ok(result);
        }

        [HttpDelete("api/sport/{sportId}")]
        public async Task<IActionResult> Delete(int sportId)
        {
            var result = await _mediator.Send(new DeleteSportCommand(sportId));
            return Ok(result);
        }

        [HttpGet("api/sport/{id}")]
        public async Task<IActionResult> GetById(int Id)
        {
            var result = await _mediator.Send(new GetSportByIdQuery(Id));
            return Ok(result);
        }

        [HttpGet("api/sports/paginated")]
        public async Task<IActionResult> GetAllPaginated(
            [FromQuery] int? page,
            [FromQuery] int? pageSize,
            CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(
                new GetAllSportsPaginatedQuery(PageRequest.Create(page, pageSize)),
                cancellationToken);
            return Ok(result);
        }

        [HttpGet("api/sports")]
        public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(
                new GetAllSportsQuery(), cancellationToken);
            return Ok(result);
        }

        [HttpGet("api/sports/available-for/branch/{branchId}")]
        public async Task<IActionResult> GetAvailableForBranch(int branchId, CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(new GetAvailableSportsForBranchQuery(branchId), cancellationToken);
            return Ok(result);
        }

        [HttpGet("api/sports/count")]
        public async Task<IActionResult> GetAllSportsCount()
        {
            var result = await _mediator.Send(new GetSportsCountQuery());
            return Ok(result);
        }

        [HttpGet("api/sports/search")]
        public async Task<IActionResult> Search(
            [FromQuery] string searchTerm,
            [FromQuery] int? page,
            [FromQuery] int? pageSize,
            CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(new SearchSportsQuery(
                searchTerm, PageRequest.Create(page, pageSize)),
                cancellationToken);
            return Ok(result);
        }

        [HttpGet("api/sports/search-name")]
        public async Task<IActionResult> SearchSportsName(
            [FromQuery] string searchTerm,
            CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(new SearchSportsNameQuery(searchTerm), cancellationToken);
            return Ok(result);
        }

        [HttpPost("api/sports/{sportId}/skill-level")]
        public async Task<IActionResult> AddSkillLevel(int sportId, [FromBody] AddSkillLevelCommand command, CancellationToken ct)
        {
            var result = await _mediator.Send(new AddSkillLevelCommand(sportId, command.Name, command.Description), ct);
            return Ok(result);
        }
    }
}
