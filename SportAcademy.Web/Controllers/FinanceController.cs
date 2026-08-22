using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SportAcademy.Application.Commands.FinanceCommands.RecordPayment;
using SportAcademy.Application.Commands.FinanceCommands.RefundPayment;
using SportAcademy.Application.Commands.FinanceCommands.VoidPayment;
using SportAcademy.Application.Common.Pagination;
using SportAcademy.Application.Queries.FinanceQueries.GetInvoiceById;
using SportAcademy.Application.Queries.FinanceQueries.GetInvoices;
using SportAcademy.Application.Queries.FinanceQueries.GetOutstandingBalances;
using SportAcademy.Application.Queries.FinanceQueries.GetPaymentReceipt;
using SportAcademy.Application.Queries.FinanceQueries.GetPayments;

namespace SportAcademy.Web.Controllers
{
    [Authorize]
    [EnableRateLimiting("per-user")]
    [Route("api/finance")]
    [ApiController]
    public class FinanceController : ControllerBase
    {
        private readonly IMediator _mediator;

        public FinanceController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [Authorize(Policy = "Permission:payment.record")]
        [HttpPost("payments")]
        public async Task<IActionResult> RecordPayment([FromBody] RecordPaymentCommand command, CancellationToken ct)
        {
            var result = await _mediator.Send(command, ct);
            return Ok(result);
        }

        [Authorize(Policy = "Permission:payment.view")]
        [HttpGet("payments")]
        public async Task<IActionResult> GetPayments(
            [FromQuery] int? page, [FromQuery] int? pageSize,
            [FromQuery] int? branchId, [FromQuery] string? method, [FromQuery] string? status,
            [FromQuery] DateTime? from, [FromQuery] DateTime? to,
            CancellationToken ct)
        {
            var result = await _mediator.Send(
                new GetPaymentsQuery(PageRequest.Create(page, pageSize), branchId, method, status, from, to), ct);
            return Ok(result);
        }

        [Authorize(Policy = "Permission:payment.view")]
        [HttpGet("payments/{paymentNumber}")]
        public async Task<IActionResult> GetPaymentReceipt(string paymentNumber, CancellationToken ct)
        {
            var result = await _mediator.Send(new GetPaymentReceiptQuery(paymentNumber), ct);
            return Ok(result);
        }

        [Authorize(Policy = "Permission:payment.refund")]
        [HttpPost("payments/{paymentNumber}/refund")]
        public async Task<IActionResult> RefundPayment(string paymentNumber, [FromBody] RefundPaymentRequest request, CancellationToken ct)
        {
            var result = await _mediator.Send(new RefundPaymentCommand(paymentNumber, request.Amount), ct);
            return Ok(result);
        }

        [Authorize(Policy = "Permission:payment.correct")]
        [HttpPost("payments/{paymentNumber}/void")]
        public async Task<IActionResult> VoidPayment(string paymentNumber, CancellationToken ct)
        {
            var result = await _mediator.Send(new VoidPaymentCommand(paymentNumber), ct);
            return Ok(result);
        }

        [Authorize(Policy = "Permission:finance.view")]
        [HttpGet("invoices")]
        public async Task<IActionResult> GetInvoices(
            [FromQuery] int? page, [FromQuery] int? pageSize,
            [FromQuery] int? branchId, [FromQuery] string? status,
            CancellationToken ct)
        {
            var result = await _mediator.Send(
                new GetInvoicesQuery(PageRequest.Create(page, pageSize), branchId, status), ct);
            return Ok(result);
        }

        [Authorize(Policy = "Permission:finance.view")]
        [HttpGet("invoices/{id}")]
        public async Task<IActionResult> GetInvoiceById(int id, CancellationToken ct)
        {
            var result = await _mediator.Send(new GetInvoiceByIdQuery(id), ct);
            return Ok(result);
        }

        [Authorize(Policy = "Permission:finance.view")]
        [HttpGet("outstanding")]
        public async Task<IActionResult> GetOutstanding(
            [FromQuery] int? page, [FromQuery] int? pageSize,
            [FromQuery] int? branchId, [FromQuery] bool overdueOnly,
            CancellationToken ct)
        {
            var result = await _mediator.Send(
                new GetOutstandingBalancesQuery(PageRequest.Create(page, pageSize), branchId, overdueOnly), ct);
            return Ok(result);
        }
    }

    public record RefundPaymentRequest(decimal Amount);
}
