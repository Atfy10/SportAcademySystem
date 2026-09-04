using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SportAcademy.Application.Commands.SubscriptionDiscountRequestCommands.ApproveSubscriptionDiscountRequest;
using SportAcademy.Application.Commands.SubscriptionDiscountRequestCommands.CreateSubscriptionDiscountRequest;
using SportAcademy.Application.Commands.SubscriptionDiscountRequestCommands.RejectSubscriptionDiscountRequest;
using SportAcademy.Application.Common.Pagination;
using SportAcademy.Application.Queries.SubscriptionDiscountRequestQueries.GetSubscriptionDiscountRequestById;
using SportAcademy.Application.Queries.SubscriptionDiscountRequestQueries.GetSubscriptionDiscountRequests;

namespace SportAcademy.Web.Controllers
{
    [Authorize]
    [EnableRateLimiting("per-user")]
    [Route("api/subscription-discount-requests")]
    [ApiController]
    public class SubscriptionDiscountRequestsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public SubscriptionDiscountRequestsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [Authorize(Policy = "Permission:discountcode.approve")]
        [HttpGet]
        public async Task<IActionResult> GetAll(
            [FromQuery] string? status, [FromQuery] int? branchId,
            [FromQuery] int? page, [FromQuery] int? pageSize, CancellationToken ct)
        {
            var result = await _mediator.Send(
                new GetSubscriptionDiscountRequestsQuery(PageRequest.Create(page, pageSize), status, branchId), ct);
            return Ok(result);
        }

        [Authorize(Policy = "Permission:discountcode.approve")]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id, CancellationToken ct)
        {
            var result = await _mediator.Send(new GetSubscriptionDiscountRequestByIdQuery(id), ct);
            return Ok(result);
        }

        [Authorize(Policy = "Permission:subscription.manage")]
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateSubscriptionDiscountRequestCommand command, CancellationToken ct)
        {
            var result = await _mediator.Send(command, ct);
            return Ok(result);
        }

        [Authorize(Policy = "Permission:discountcode.approve")]
        [HttpPost("{id}/approve")]
        public async Task<IActionResult> Approve(int id, CancellationToken ct)
        {
            var result = await _mediator.Send(new ApproveSubscriptionDiscountRequestCommand(id), ct);
            return Ok(result);
        }

        [Authorize(Policy = "Permission:discountcode.approve")]
        [HttpPost("{id}/reject")]
        public async Task<IActionResult> Reject(int id, [FromBody] RejectSubscriptionDiscountRequestBody body, CancellationToken ct)
        {
            var result = await _mediator.Send(new RejectSubscriptionDiscountRequestCommand(id, body.RejectionReason), ct);
            return Ok(result);
        }
    }

    public record RejectSubscriptionDiscountRequestBody(string RejectionReason);
}
