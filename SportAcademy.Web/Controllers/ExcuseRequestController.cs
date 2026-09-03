using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SportAcademy.Application.Commands.ExcuseRequestCommands.ApproveExcuseRequest;
using SportAcademy.Application.Commands.ExcuseRequestCommands.CreateExcuseRequest;
using SportAcademy.Application.Commands.ExcuseRequestCommands.RejectExcuseRequest;
using SportAcademy.Application.Common.Pagination;
using SportAcademy.Application.Queries.ExcuseRequestQueries.CountPending;
using SportAcademy.Application.Queries.ExcuseRequestQueries.GetPending;

namespace SportAcademy.Web.Controllers
{
[Authorize]
[EnableRateLimiting("per-user")]
[Route("api/[controller]")]
    [ApiController]
    public class ExcuseRequestController : ControllerBase
    {
        private readonly IMediator _mediator;

        public ExcuseRequestController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost]
        [Authorize(Policy = "Permission:attendance.mark")]
        public async Task<IActionResult> Create([FromBody] CreateExcuseRequestCommand command, CancellationToken ct)
        {
            var result = await _mediator.Send(command, ct);
            return Ok(result);
        }

        [HttpGet("pending/count")]
        [Authorize(Policy = "Permission:attendance.approve_excuse")]
        public async Task<IActionResult> CountPending(CancellationToken ct)
        {
            var result = await _mediator.Send(new CountPendingExcuseRequestsQuery(), ct);
            return Ok(result);
        }

        [HttpGet("pending")]
        [Authorize(Policy = "Permission:attendance.approve_excuse")]
        public async Task<IActionResult> GetPending(
            [FromQuery] int? page,
            [FromQuery] int? pageSize,
            CancellationToken ct)
        {
            var result = await _mediator.Send(new GetPendingExcuseRequestsQuery(PageRequest.Create(page, pageSize)), ct);
            return Ok(result);
        }

        [HttpPatch("{id}/approve")]
        [Authorize(Policy = "Permission:attendance.approve_excuse")]
        public async Task<IActionResult> Approve([FromRoute] int id, CancellationToken ct)
        {
            var result = await _mediator.Send(new ApproveExcuseRequestCommand(id), ct);
            return Ok(result);
        }

        [HttpPatch("{id}/reject")]
        [Authorize(Policy = "Permission:attendance.approve_excuse")]
        public async Task<IActionResult> Reject([FromRoute] int id, [FromBody] RejectExcuseRequestBody? body, CancellationToken ct)
        {
            var result = await _mediator.Send(new RejectExcuseRequestCommand(id, body?.Note), ct);
            return Ok(result);
        }
    }

    public record RejectExcuseRequestBody(string? Note);
}
