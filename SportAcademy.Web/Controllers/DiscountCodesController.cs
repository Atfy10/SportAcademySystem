using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SportAcademy.Application.Commands.DiscountCodeCommands.CreateDiscountCode;
using SportAcademy.Application.Commands.DiscountCodeCommands.DeleteDiscountCode;
using SportAcademy.Application.Commands.DiscountCodeCommands.UpdateDiscountCode;
using SportAcademy.Application.Queries.DiscountCodeQueries.GetAll;
using SportAcademy.Application.Queries.DiscountCodeQueries.ValidateDiscountCode;

namespace SportAcademy.Web.Controllers
{
    [Authorize]
    [EnableRateLimiting("per-user")]
    [Route("api/discount-codes")]
    [ApiController]
    public class DiscountCodesController : ControllerBase
    {
        private readonly IMediator _mediator;

        public DiscountCodesController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [Authorize(Policy = "Permission:discountcode.manage")]
        [HttpGet]
        public async Task<IActionResult> GetAll(CancellationToken ct)
        {
            var result = await _mediator.Send(new GetAllDiscountCodesQuery(), ct);
            return Ok(result);
        }

        [Authorize(Policy = "Permission:discountcode.manage")]
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateDiscountCodeCommand command, CancellationToken ct)
        {
            var result = await _mediator.Send(command, ct);
            return Ok(result);
        }

        [Authorize(Policy = "Permission:discountcode.manage")]
        [HttpPut]
        public async Task<IActionResult> Update([FromBody] UpdateDiscountCodeCommand command, CancellationToken ct)
        {
            var result = await _mediator.Send(command, ct);
            return Ok(result);
        }

        [Authorize(Policy = "Permission:discountcode.manage")]
        [HttpDelete]
        public async Task<IActionResult> Delete([FromBody] DeleteDiscountCodeCommand command, CancellationToken ct)
        {
            var result = await _mediator.Send(command, ct);
            if (!result.IsSuccess)
                return BadRequest(result.Message);

            return NoContent();
        }

        // subscription.manage (not discountcode.manage) - this is the redemption-preview lookup
        // used from SubscriptionFormModal while creating a subscription, so whoever can create a
        // subscription (Employee included) can preview a code, without also being able to mint
        // or manage codes.
        [Authorize(Policy = "Permission:subscription.manage")]
        [HttpGet("validate")]
        public async Task<IActionResult> Validate([FromQuery] string code, CancellationToken ct)
        {
            var result = await _mediator.Send(new ValidateDiscountCodeQuery(code), ct);
            return Ok(result);
        }
    }
}
