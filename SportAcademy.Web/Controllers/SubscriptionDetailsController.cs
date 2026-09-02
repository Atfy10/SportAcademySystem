using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SportAcademy.Application.Commands.SubscriptionDetailsCommands.ActivateSubscription;
using SportAcademy.Application.Commands.SubscriptionDetailsCommands.CreateSubscriptionDetails;
using SportAcademy.Application.Commands.SubscriptionDetailsCommands.DeleteSubscriptionDetails;
using SportAcademy.Application.Commands.SubscriptionDetailsCommands.SuspendSubscription;
using SportAcademy.Application.Commands.SubscriptionDetailsCommands.UpdateSubscriptionDetails;
using SportAcademy.Application.Common.Pagination;
using SportAcademy.Application.Queries.SubscriptionDetailsQueries.GetAllForDropdown;
using SportAcademy.Application.Queries.SubscriptionDetailsQueries.GetAllPaginated;
using SportAcademy.Application.Queries.SubscriptionDetailsQueries.GetById;
using SportAcademy.Application.Queries.SubscriptionDetailsQueries.GetByTraineeId;
using SportAcademy.Application.Queries.SubscriptionDetailsQueries.GetLatest;
using SportAcademy.Application.Queries.SubscriptionDetailsQueries.GetRenewInfo;
using SportAcademy.Application.Queries.SubscriptionDetailsQueries.GetStats;

namespace SportAcademy.Web.Controllers
{
[Authorize]
[EnableRateLimiting("per-user")]
[Route("api/[controller]")]
    [ApiController]
    public class SubscriptionDetailsController : ControllerBase
    {
        IMediator _mediator;

        public SubscriptionDetailsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        public async Task<IActionResult> Index(
            [FromQuery] int? page,
            [FromQuery] int? pageSize,
            [FromQuery] string? term,
            CancellationToken ct)
        {
            var result = await _mediator.Send(
                new GetAllSubDetailsPaginatedQuery(PageRequest.Create(page, pageSize), term), ct);
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Details(int id)
        {
            var result = await _mediator.Send(new GetSubDetailsByIdQuery(id));
            return Ok(result);
        }

        [HttpGet("trainee/{traineeId}/history")]
        public async Task<IActionResult> GetHistoryForTrainee(int traineeId, CancellationToken ct)
        {
            var result = await _mediator.Send(new GetSubscriptionsByTraineeIdQuery(traineeId), ct);
            return Ok(result);
        }

        [Authorize(Policy = "Permission:subscription.manage")]
        [HttpPost]
        public async Task<IActionResult> CreateAsync(CreateSubscriptionDetailsCommand command)
        {
            var result = await _mediator.Send(command);
            if (!result.IsSuccess)
                return BadRequest(result);

            return Ok(result);
        }

        [Authorize(Policy = "Permission:subscription.manage")]
        [HttpPut]
        public async Task<IActionResult> EditAsync(UpdateSubscriptionDetailsCommand command)
        {
            var result = await _mediator.Send(command);
            return Ok(result);
        }

        [Authorize(Policy = "Permission:subscription.manage")]
        [HttpDelete]
        public async Task<IActionResult> Delete(DeleteSubscriptionDetailsCommand command, CancellationToken ct)
        {
            var result = await _mediator.Send(command, ct);
            if (!result.IsSuccess)
                return BadRequest(result.Message);

            return NoContent();
        }

        [HttpGet("dropdown")]
        public async Task<IActionResult> GetDropdown([FromQuery] int? traineeId, CancellationToken ct)
        {
            var result = await _mediator.Send(new GetAllSubscriptionDetailsForDropdownQuery(traineeId), ct);
            return Ok(result);
        }

        [HttpGet("stats")]
        public async Task<IActionResult> GetStats(CancellationToken ct)
        {
            var result = await _mediator.Send(new GetSubDetailsStatsQuery(), ct);

            if (!result.IsSuccess)
                return BadRequest(result);

            return Ok(result);
        }

        [HttpGet("latest")]
        public async Task<IActionResult> GetLatest(
            [FromQuery] int? page,
            [FromQuery] int? pageSize,
            [FromQuery] string? term,
            CancellationToken ct)
        {
            var result = await _mediator.Send(
                new GetLatestSubDetailsQuery(PageRequest.Create(page, pageSize), term), ct);
            return Ok(result);
        }

        [HttpGet("{id}/renew-info")]
        public async Task<IActionResult> GetRenewInfo(int id, CancellationToken ct)
        {
            var result = await _mediator.Send(new GetRenewSubscriptionInfoQuery(id), ct);

            if (!result.IsSuccess)
                return BadRequest(result);

            return Ok(result);
        }

        [Authorize(Policy = "Permission:subscription.manage")]
        [HttpPatch("{id}/suspend")]
        public async Task<IActionResult> Suspend(int id, CancellationToken ct)
        {
            var result = await _mediator.Send(new SuspendSubscriptionCommand(id), ct);

            if (!result.IsSuccess)
                return BadRequest(result);

            return Ok(result);
        }

        [Authorize(Policy = "Permission:subscription.manage")]
        [HttpPatch("{id}/activate")]
        public async Task<IActionResult> Activate(int id, CancellationToken ct)
        {
            var result = await _mediator.Send(new ActivateSubscriptionCommand(id), ct);

            if (!result.IsSuccess)
                return BadRequest(result);

            return Ok(result);
        }
    }
}


