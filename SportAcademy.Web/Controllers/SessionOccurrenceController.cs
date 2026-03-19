using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SportAcademy.Application.Commands.SessionOccurrenceCommands.CreateSessionOccurrence;
using SportAcademy.Application.Commands.SessionOccurrenceCommands.DeleteSessionOccurence;
using SportAcademy.Application.Commands.SessionOccurrenceCommands.UpdateSessionOccurrence;
using SportAcademy.Application.Common.Pagination;
using SportAcademy.Application.Queries.SessionOccurrenceQueries.CountAll;
using SportAcademy.Application.Queries.SessionOccurrenceQueries.GetAll;
using SportAcademy.Application.Queries.SessionOccurrenceQueries.GetById;
using SportAcademy.Application.Queries.SessionOccurrenceQueries.GetPaginated;
using SportAcademy.Application.Queries.SessionOccurrenceQueries.SearchSessionOccurrences;

namespace SportAcademy.Web.Controllers
{
    [Authorize]
    [ApiController]
    public class SessionOccurrenceController : ControllerBase
    {
        private readonly IMediator _mediator;

        public SessionOccurrenceController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost("api/session-occurrence")]
        public async Task<IActionResult> Create([FromBody] CreateSessionOccurrenceCommand command, CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(command, cancellationToken);
            return Ok(result);
        }

        [HttpGet("api/session-occurrence")]
        public async Task<IActionResult> GetAll(
            [FromQuery] int? page,
            [FromQuery] int? pageSize,
            CancellationToken cancellationToken)
        {
            var pageReq = PageRequest.Create(page, pageSize);
            var result = await _mediator.Send(new GetPaginatedSessionOccurrencesQuery(pageReq), cancellationToken);
            return Ok(result);
        }

        [HttpGet("api/session-occurrence/by-date")]
        public async Task<IActionResult> GetByDate(
            [FromQuery] DateTime date,
            [FromQuery] int? page,
            [FromQuery] int? pageSize,
            CancellationToken cancellationToken)
        {
            var pageReq = PageRequest.Create(page, pageSize);
            var result = await _mediator.Send(new GetSessionOccurrencesByDateQuery(date, pageReq), cancellationToken);
            return Ok(result);
        }

        [HttpGet("api/session-occurrence/search")]
        public async Task<IActionResult> Search(
            [FromQuery] string term,
            [FromQuery] int? page,
            [FromQuery] int? pageSize,
            CancellationToken cancellationToken)
        {
            var pageReq = PageRequest.Create(page, pageSize);
            var result = await _mediator.Send(new SearchSessionOccurrencesQuery(term, pageReq), cancellationToken);
            return Ok(result);
        }

        [HttpGet("api/session-occurrence/{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _mediator.Send(new GetSessionOccurrenceByIdQuery(id));
            return Ok(result);
        }

        [HttpPut("api/session-occurrence")]
        public async Task<IActionResult> Update([FromBody] UpdateSessionOccurrenceCommand command, CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(command, cancellationToken);
            return Ok(result);
        }

        [HttpDelete("api/session-occurrence/{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _mediator.Send(new DeleteSessionOccurrenceCommand(id));
            return Ok(result);
        }

        [HttpGet("api/session-occurrence/count")]
        public async Task<IActionResult> Count(CancellationToken ct)
        {
            var result = await _mediator.Send(new CountSessionOccurrencesQuery(), ct);
            return Ok(result);
        }
    }
}
