using CsvHelper;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SportAcademy.Application.Queries.ReportQueries.GetOutstandingReport;
using SportAcademy.Application.Queries.ReportQueries.GetPaymentMethodReport;
using SportAcademy.Application.Queries.ReportQueries.GetRevenueReport;
using System.Globalization;

namespace SportAcademy.Web.Controllers
{
    [Authorize(Policy = "Permission:report.view")]
    [EnableRateLimiting("per-user")]
    [Route("api/reports")]
    [ApiController]
    public class ReportsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public ReportsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet("revenue")]
        public async Task<IActionResult> GetRevenue(
            [FromQuery] DateTime? from, [FromQuery] DateTime? to,
            [FromQuery] int? branchId, [FromQuery] string? groupBy,
            [FromQuery] string? format,
            CancellationToken ct)
        {
            var result = await _mediator.Send(new GetRevenueReportQuery(from, to, branchId, groupBy), ct);

            if (string.Equals(format, "csv", StringComparison.OrdinalIgnoreCase) && result.IsSuccess)
                return WriteCsv(result.Data!, "revenue-report.csv");

            return Ok(result);
        }

        [HttpGet("outstanding")]
        public async Task<IActionResult> GetOutstanding([FromQuery] int? branchId, CancellationToken ct)
        {
            var result = await _mediator.Send(new GetOutstandingReportQuery(branchId), ct);
            return Ok(result);
        }

        [HttpGet("payment-methods")]
        public async Task<IActionResult> GetPaymentMethods(
            [FromQuery] DateTime? from, [FromQuery] DateTime? to, [FromQuery] int? branchId,
            [FromQuery] string? format,
            CancellationToken ct)
        {
            var result = await _mediator.Send(new GetPaymentMethodReportQuery(from, to, branchId), ct);

            if (string.Equals(format, "csv", StringComparison.OrdinalIgnoreCase) && result.IsSuccess)
                return WriteCsv(result.Data!, "payment-methods-report.csv");

            return Ok(result);
        }

        // report.export is granted alongside report.view to every role that can reach this
        // controller (see AppDataSeeder.DefaultRolePermissions), so CSV export is gated by the
        // same class-level policy rather than a second check per action.
        private FileContentResult WriteCsv<T>(IEnumerable<T> rows, string fileName)
        {
            using var stream = new MemoryStream();
            using (var writer = new StreamWriter(stream, leaveOpen: true))
            using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
            {
                csv.WriteRecords(rows);
            }

            return File(stream.ToArray(), "text/csv", fileName);
        }
    }
}
