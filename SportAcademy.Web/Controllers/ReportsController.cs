using CsvHelper;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SportAcademy.Application.Common.Pagination;
using SportAcademy.Application.Queries.ReportQueries.GetAttendanceReport;
using SportAcademy.Application.Queries.ReportQueries.GetOutstandingReport;
using SportAcademy.Application.Queries.ReportQueries.GetPaymentMethodReport;
using SportAcademy.Application.Queries.ReportQueries.GetRevenueReport;
using SportAcademy.Application.Queries.ReportQueries.GetSubscriptionsReport;
using System.Globalization;

namespace SportAcademy.Web.Controllers
{
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

        [Authorize(Policy = "Permission:report.view")]
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

        [Authorize(Policy = "Permission:report.view")]
        [HttpGet("outstanding")]
        public async Task<IActionResult> GetOutstanding([FromQuery] int? branchId, CancellationToken ct)
        {
            var result = await _mediator.Send(new GetOutstandingReportQuery(branchId), ct);
            return Ok(result);
        }

        [Authorize(Policy = "Permission:report.view")]
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

        // Attendance/Subscriptions reports carry their own, narrower permissions
        // (report.view.attendance / report.view.subscriptions) instead of report.view, so
        // Employee (staff) can be granted access to just these two without also unlocking the
        // financial reports above. Omitting page/pageSize returns every matching row/session
        // (capped at 5000 server-side) - CSV export relies on this to get the full filtered list
        // in one call instead of paging through it. Printing, in contrast, only ever prints one
        // already-loaded session at a time (see Reports.tsx), so it doesn't use this mode.
        [Authorize(Policy = "Permission:report.view.attendance")]
        [HttpGet("attendance")]
        public async Task<IActionResult> GetAttendanceReport(
            [FromQuery] DateTime? from, [FromQuery] DateTime? to, [FromQuery] int? branchId,
            [FromQuery] int? traineeGroupId, [FromQuery] int? traineeId, [FromQuery] int? coachId,
            [FromQuery] string? status, [FromQuery] int? page, [FromQuery] int? pageSize,
            [FromQuery] string? format, CancellationToken ct)
        {
            var pageRequest = page.HasValue ? PageRequest.Create(page.Value, pageSize) : null;
            var result = await _mediator.Send(
                new GetAttendanceReportQuery(from, to, branchId, traineeGroupId, traineeId, coachId, status, pageRequest), ct);

            if (string.Equals(format, "csv", StringComparison.OrdinalIgnoreCase) && result.IsSuccess)
                return WriteCsv(result.Data!.Items.SelectMany(g => g.Trainees.Select(t => new
                {
                    g.AttendanceDate,
                    g.TraineeGroupName,
                    g.BranchName,
                    g.CoachName,
                    t.TraineeName,
                    t.Status,
                    t.CheckInTime,
                    t.CoachNote,
                })), "attendance-report.csv");

            return Ok(result);
        }

        [Authorize(Policy = "Permission:report.view.subscriptions")]
        [HttpGet("subscriptions")]
        public async Task<IActionResult> GetSubscriptionsReport(
            [FromQuery] DateTime? from, [FromQuery] DateTime? to, [FromQuery] int? branchId,
            [FromQuery] int? sportId, [FromQuery] string? status, [FromQuery] int? page,
            [FromQuery] int? pageSize, [FromQuery] string? format, CancellationToken ct)
        {
            var pageRequest = page.HasValue ? PageRequest.Create(page.Value, pageSize) : null;
            var result = await _mediator.Send(
                new GetSubscriptionsReportQuery(from, to, branchId, sportId, status, pageRequest), ct);

            if (string.Equals(format, "csv", StringComparison.OrdinalIgnoreCase) && result.IsSuccess)
                return WriteCsv(result.Data!.Items.Select(d => new
                {
                    Trainee = d.Trainee.FullName,
                    d.SportName,
                    d.BranchName,
                    d.SubscriptionTypeName,
                    d.Price,
                    d.StartDate,
                    d.EndDate,
                    Status = d.Status.ToString(),
                    d.EmployeeName,
                }), "subscriptions-report.csv");

            return Ok(result);
        }

        // report.export is granted alongside report.view to every role that can reach the
        // financial-report actions above (see AppDataSeeder.DefaultRolePermissions), so their
        // CSV export needs no extra check; the attendance/subscriptions actions gate CSV export
        // with the same per-action policy as the JSON response, since they use narrower
        // permissions than report.view/report.export.
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
