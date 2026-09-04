using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SportAcademy.Application.Commands.SalaryPaymentCommands.ApproveSalaryPayment;
using SportAcademy.Application.Commands.SalaryPaymentCommands.CreateSalaryPayment;
using SportAcademy.Application.Commands.SalaryPaymentCommands.DeleteSalaryPayment;
using SportAcademy.Application.Commands.SalaryPaymentCommands.MarkSalaryPaymentPaid;
using SportAcademy.Application.Commands.SalaryPaymentCommands.RejectSalaryPayment;
using SportAcademy.Application.Common.Pagination;
using SportAcademy.Application.Queries.SalaryPaymentQueries.GetPayrollEmployees;
using SportAcademy.Application.Queries.SalaryPaymentQueries.GetSalaryPaymentById;
using SportAcademy.Application.Queries.SalaryPaymentQueries.GetSalaryPayments;

namespace SportAcademy.Web.Controllers
{
    [Authorize]
    [EnableRateLimiting("per-user")]
    [Route("api/salary")]
    [ApiController]
    public class SalaryController : ControllerBase
    {
        private readonly IMediator _mediator;

        public SalaryController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [Authorize(Policy = "Permission:salary.view")]
        [HttpGet("employees")]
        public async Task<IActionResult> GetEmployees([FromQuery] int? branchId, [FromQuery] string? search, CancellationToken ct)
        {
            var result = await _mediator.Send(new GetPayrollEmployeesQuery(branchId, search), ct);
            return Ok(result);
        }

        [Authorize(Policy = "Permission:salary.view")]
        [HttpGet("payments")]
        public async Task<IActionResult> GetPayments(
            [FromQuery] int? employeeId, [FromQuery] int? branchId, [FromQuery] string? status,
            [FromQuery] DateOnly? periodFrom, [FromQuery] DateOnly? periodTo,
            [FromQuery] int? page, [FromQuery] int? pageSize,
            CancellationToken ct)
        {
            var result = await _mediator.Send(
                new GetSalaryPaymentsQuery(
                    PageRequest.Create(page, pageSize), employeeId, branchId, status, periodFrom, periodTo), ct);
            return Ok(result);
        }

        [Authorize(Policy = "Permission:salary.view")]
        [HttpGet("payments/{id}")]
        public async Task<IActionResult> GetById(int id, CancellationToken ct)
        {
            var result = await _mediator.Send(new GetSalaryPaymentByIdQuery(id), ct);
            return Ok(result);
        }

        [Authorize(Policy = "Permission:salary.create")]
        [HttpPost("payments")]
        public async Task<IActionResult> Create([FromBody] CreateSalaryPaymentCommand command, CancellationToken ct)
        {
            var result = await _mediator.Send(command, ct);
            return Ok(result);
        }

        [Authorize(Policy = "Permission:salary.approve")]
        [HttpPost("payments/{id}/approve")]
        public async Task<IActionResult> Approve(int id, CancellationToken ct)
        {
            var result = await _mediator.Send(new ApproveSalaryPaymentCommand(id), ct);
            return Ok(result);
        }

        [Authorize(Policy = "Permission:salary.approve")]
        [HttpPost("payments/{id}/reject")]
        public async Task<IActionResult> Reject(int id, [FromBody] RejectSalaryPaymentRequest request, CancellationToken ct)
        {
            var result = await _mediator.Send(new RejectSalaryPaymentCommand(id, request.RejectionReason), ct);
            return Ok(result);
        }

        [Authorize(Policy = "Permission:salary.markpaid")]
        [HttpPost("payments/{id}/mark-paid")]
        public async Task<IActionResult> MarkPaid(int id, CancellationToken ct)
        {
            var result = await _mediator.Send(new MarkSalaryPaymentPaidCommand(id), ct);
            return Ok(result);
        }

        [Authorize(Policy = "Permission:salary.create")]
        [HttpDelete("payments/{id}")]
        public async Task<IActionResult> Delete(int id, CancellationToken ct)
        {
            var result = await _mediator.Send(new DeleteSalaryPaymentCommand(id), ct);
            if (!result.IsSuccess)
                return BadRequest(result.Message);

            return NoContent();
        }
    }

    public record RejectSalaryPaymentRequest(string RejectionReason);
}
