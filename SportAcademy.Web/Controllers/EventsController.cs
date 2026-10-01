using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SportAcademy.Application.Commands.EventCommands.CancelEvent;
using SportAcademy.Application.Commands.EventCommands.CreateEvent;
using SportAcademy.Application.Commands.EventCommands.DeleteEvent;
using SportAcademy.Application.Commands.EventCommands.RegenerateEventEntryCode;
using SportAcademy.Application.Commands.EventCommands.UpdateEvent;
using SportAcademy.Application.Common.Pagination;
using SportAcademy.Application.Queries.EventQueries.GetEventById;
using SportAcademy.Application.Queries.EventQueries.GetEventOverlaps;
using SportAcademy.Application.Queries.EventQueries.GetEvents;
using SportAcademy.Application.Queries.EventQueries.GetEventsReport;
using SportAcademy.Domain.Enums;

namespace SportAcademy.Web.Controllers
{
    // Venue bookings (the "event-management" feature - Enterprise plan). Every command/query
    // behind this controller is feature-gated in the MediatR pipeline, so a tenant without the
    // feature gets FEATURE_DISABLED (403) whatever its permissions.
    [Authorize]
    [EnableRateLimiting("per-user")]
    [Route("api/events")]
    [ApiController]
    public class EventsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public EventsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [Authorize(Policy = "Permission:event.view")]
        [HttpGet]
        public async Task<IActionResult> GetEvents(
            [FromQuery] int? branchId, [FromQuery] DateOnly? from, [FromQuery] DateOnly? to,
            [FromQuery] EventStatus? status, [FromQuery] int? customerId, [FromQuery] Guid? createdByUserId,
            [FromQuery] string? term, [FromQuery] int? page, [FromQuery] int? pageSize,
            CancellationToken ct)
        {
            var result = await _mediator.Send(new GetEventsQuery(
                PageRequest.Create(page, pageSize), branchId, from, to, status, customerId, createdByUserId, term), ct);
            return Ok(result);
        }

        [Authorize(Policy = "Permission:event.report")]
        [HttpGet("report")]
        public async Task<IActionResult> GetReport(
            [FromQuery] DateOnly? from, [FromQuery] DateOnly? to, [FromQuery] int? branchId,
            [FromQuery] EventStatus? status, [FromQuery] int? customerId, [FromQuery] Guid? createdByUserId,
            CancellationToken ct)
        {
            var result = await _mediator.Send(
                new GetEventsReportQuery(from, to, branchId, status, customerId, createdByUserId), ct);
            return Ok(result);
        }

        [Authorize(Policy = "Permission:event.view")]
        [HttpGet("overlaps")]
        public async Task<IActionResult> GetOverlaps(
            [FromQuery] int branchId, [FromQuery] DateTime startsAt, [FromQuery] DateTime endsAt,
            [FromQuery] int? excludeId, CancellationToken ct)
        {
            var result = await _mediator.Send(new GetEventOverlapsQuery(branchId, startsAt, endsAt, excludeId), ct);
            return Ok(result);
        }

        [Authorize(Policy = "Permission:event.view")]
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id, CancellationToken ct)
        {
            var result = await _mediator.Send(new GetEventByIdQuery(id), ct);
            return Ok(result);
        }

        [Authorize(Policy = "Permission:event.manage")]
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateEventCommand command, CancellationToken ct)
        {
            var result = await _mediator.Send(command, ct);
            return Ok(result);
        }

        [Authorize(Policy = "Permission:event.manage")]
        [HttpPut]
        public async Task<IActionResult> Update([FromBody] UpdateEventCommand command, CancellationToken ct)
        {
            var result = await _mediator.Send(command, ct);
            return Ok(result);
        }

        [Authorize(Policy = "Permission:event.manage")]
        [HttpPost("{id:int}/cancel")]
        public async Task<IActionResult> Cancel(int id, [FromBody] CancelEventBody body, CancellationToken ct)
        {
            var result = await _mediator.Send(new CancelEventCommand(id, body.Reason, body.Mode), ct);
            return Ok(result);
        }

        // Replaces the entry QR code - the old one stops working at once.
        [Authorize(Policy = "Permission:event.manage")]
        [HttpPost("{id:int}/entry-code")]
        public async Task<IActionResult> RegenerateEntryCode(int id, CancellationToken ct)
        {
            var result = await _mediator.Send(new RegenerateEventEntryCodeCommand(id), ct);
            return Ok(result);
        }

        [Authorize(Policy = "Permission:event.manage")]
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id, CancellationToken ct)
        {
            var result = await _mediator.Send(new DeleteEventCommand(id), ct);
            return Ok(result);
        }

        public record CancelEventBody(string Reason, EventCancellationMode Mode);
    }
}
