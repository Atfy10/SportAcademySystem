using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SportAcademy.Application.Commands.EventCommands.CancelEvent;
using SportAcademy.Application.Commands.EventCommands.CreateEvent;
using SportAcademy.Application.Commands.EventCommands.DeleteEvent;
using SportAcademy.Application.Commands.EventCommands.UpdateEvent;
using SportAcademy.Application.Commands.EventTicketCommands.AdmitEventTicket;
using SportAcademy.Application.Commands.EventTicketCommands.IssueEventTickets;
using SportAcademy.Application.Commands.EventTicketCommands.ReissueEventTicket;
using SportAcademy.Application.Commands.EventTicketCommands.RevokeEventTicket;
using SportAcademy.Application.Commands.EventTicketCommands.UpdateEventTicket;
using SportAcademy.Application.Common.Pagination;
using SportAcademy.Application.Queries.EventQueries.GetEventById;
using SportAcademy.Application.Queries.EventQueries.GetEventOverlaps;
using SportAcademy.Application.Queries.EventQueries.GetEvents;
using SportAcademy.Application.Queries.EventQueries.GetEventsReport;
using SportAcademy.Application.Queries.EventTicketQueries.CheckEventTicket;
using SportAcademy.Application.Queries.EventTicketQueries.GetCheckInEvents;
using SportAcademy.Application.Queries.EventTicketQueries.GetEventTickets;
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

        [Authorize(Policy = "Permission:event.manage")]
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id, CancellationToken ct)
        {
            var result = await _mediator.Send(new DeleteEventCommand(id), ct);
            return Ok(result);
        }

        // -- Tickets: each guest gets a numbered ticket whose QR code staff scan at the door.

        [Authorize(Policy = "Permission:event.view")]
        [HttpGet("{id:int}/tickets")]
        public async Task<IActionResult> GetTickets(
            int id, [FromQuery] EventTicketFilter? filter, [FromQuery] string? term,
            [FromQuery] int? page, [FromQuery] int? pageSize, CancellationToken ct)
        {
            var result = await _mediator.Send(new GetEventTicketsQuery(
                id, filter ?? EventTicketFilter.All, term, PageRequest.Create(page, pageSize)), ct);
            return Ok(result);
        }

        [Authorize(Policy = "Permission:event.manage")]
        [HttpPost("{id:int}/tickets")]
        public async Task<IActionResult> IssueTickets(int id, [FromBody] IssueTicketsBody body, CancellationToken ct)
        {
            var result = await _mediator.Send(new IssueEventTicketsCommand(id, body.Count, body.GuestName), ct);
            return Ok(result);
        }

        [Authorize(Policy = "Permission:event.manage")]
        [HttpPut("tickets/{ticketId:int}")]
        public async Task<IActionResult> UpdateTicket(int ticketId, [FromBody] UpdateTicketBody body, CancellationToken ct)
        {
            var result = await _mediator.Send(new UpdateEventTicketCommand(ticketId, body.GuestName), ct);
            return Ok(result);
        }

        // New code for an unused ticket - the old one stops working at once.
        [Authorize(Policy = "Permission:event.manage")]
        [HttpPost("tickets/{ticketId:int}/reissue")]
        public async Task<IActionResult> ReissueTicket(int ticketId, CancellationToken ct)
        {
            var result = await _mediator.Send(new ReissueEventTicketCommand(ticketId), ct);
            return Ok(result);
        }

        [Authorize(Policy = "Permission:event.manage")]
        [HttpDelete("tickets/{ticketId:int}")]
        public async Task<IActionResult> RevokeTicket(int ticketId, CancellationToken ct)
        {
            var result = await _mediator.Send(new RevokeEventTicketCommand(ticketId), ct);
            return Ok(result);
        }

        // -- Door check-in: staff scan a guest's ticket (check), then let them in (admit) - or
        // refuse, which records nothing.

        [Authorize(Policy = "Permission:event.checkin")]
        [HttpGet("check-in/events")]
        public async Task<IActionResult> GetCheckInEvents(CancellationToken ct)
        {
            var result = await _mediator.Send(new GetCheckInEventsQuery(), ct);
            return Ok(result);
        }

        [Authorize(Policy = "Permission:event.checkin")]
        [HttpPost("check-in/check")]
        public async Task<IActionResult> CheckTicket([FromBody] TicketReferenceBody body, CancellationToken ct)
        {
            var result = await _mediator.Send(new CheckEventTicketQuery(body.Code, body.EventId, body.Number), ct);
            return Ok(result);
        }

        [Authorize(Policy = "Permission:event.checkin")]
        [HttpPost("check-in/admit")]
        public async Task<IActionResult> AdmitTicket([FromBody] TicketReferenceBody body, CancellationToken ct)
        {
            var result = await _mediator.Send(new AdmitEventTicketCommand(body.Code, body.EventId, body.Number), ct);
            return Ok(result);
        }

        public record CancelEventBody(string Reason, EventCancellationMode Mode);
        public record IssueTicketsBody(int Count, string? GuestName);
        public record UpdateTicketBody(string? GuestName);
        // The scanned QR (its link or bare token) - or, when it can't be scanned, event + number.
        public record TicketReferenceBody(string? Code, int? EventId, int? Number);
    }
}
