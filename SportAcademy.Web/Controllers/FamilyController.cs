using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SportAcademy.Application.Commands.FamilyCommands.AddTraineeToFamily;
using SportAcademy.Application.Commands.FamilyCommands.UpdateFamily;
using SportAcademy.Application.Common.Pagination;
using SportAcademy.Application.Queries.FamilyQueries.GetAllFamilies;
using SportAcademy.Application.Queries.FamilyQueries.GetFamilyById;
using SportAcademy.Application.Queries.FamilyQueries.SearchFamily;

namespace SportAcademy.Web.Controllers
{
    [Authorize]
    [EnableRateLimiting("per-user")]
[Route("api/[controller]")]
    [ApiController]
    public class FamilyController : ControllerBase
    {
        private readonly IMediator _mediator;

        public FamilyController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        public async Task<IActionResult> GetFamilies(
            [FromQuery] int? page,
            [FromQuery] int? pageSize,
            CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(
                new GetAllFamiliesQuery(PageRequest.Create(page, pageSize)),
                cancellationToken);
            return Ok(result);
        }

        [HttpGet("search")]
        public async Task<IActionResult> GetFamilies(
            [FromQuery] string searchTerm,
            CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(
                new SearchFamilyQuery(searchTerm),
                cancellationToken);
            return Ok(result);
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetFamilyById(int id, CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(new GetFamilyByIdQuery(id), cancellationToken);
            return Ok(result);
        }

        [HttpPut("{id:int}")]
        [Authorize(Policy = "Permission:trainee.edit")]
        public async Task<IActionResult> UpdateFamily(
            int id,
            [FromBody] UpdateFamilyCommand command,
            CancellationToken cancellationToken)
        {
            if (id != command.Id)
                return BadRequest();

            var result = await _mediator.Send(command, cancellationToken);
            return Ok(result);
        }

        [HttpPost("{id:int}/members")]
        [Authorize(Policy = "Permission:trainee.edit")]
        public async Task<IActionResult> AddTraineeToFamily(
            int id,
            [FromBody] AddTraineeToFamilyCommand command,
            CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(command with { FamilyId = id }, cancellationToken);
            return Ok(result);
        }
    }
}
