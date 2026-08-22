using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SportAcademy.Application.Commands.PaymentCommands.UpdatePayment;
using SportAcademy.Application.Queries.PaymentQueries.GetHistoryForTrainee;

namespace SportAcademy.Web.Controllers
{
    [Authorize]
    [EnableRateLimiting("per-user")]
    [Route("api/[controller]")]
    [ApiController]
    public class PaymentController : ControllerBase
    {
        private readonly IMediator _mediator;

        public PaymentController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [Authorize(Policy = "Permission:payment.view")]
        [HttpGet("trainee/{traineeId}/history")]
        public async Task<IActionResult> GetHistoryForTrainee(int traineeId, CancellationToken ct)
        {
            var result = await _mediator.Send(new GetPaymentHistoryForTraineeQuery(traineeId), ct);
            return Ok(result);
        }

        [HttpPut("{paymentNumber}")]
        [Authorize(Policy = "Permission:payment.correct")]
        public async Task<IActionResult> Update(string paymentNumber, [FromBody] UpdatePaymentCommand command, CancellationToken ct)
        {
            var result = await _mediator.Send(command with { PaymentNumber = paymentNumber }, ct);
            return Ok(result);
        }
    }
}
