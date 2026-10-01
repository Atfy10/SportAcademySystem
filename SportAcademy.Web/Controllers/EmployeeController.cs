using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SportAcademy.Application.Commands.EmployeeCommands.ChangeEmployeePosition;
using SportAcademy.Application.Commands.EmployeeCommands.CreateEmployee;
using SportAcademy.Application.Commands.EmployeeCommands.DeleteEmployee;
using SportAcademy.Application.Commands.EmployeeCommands.ImportEmployees;
using SportAcademy.Application.Commands.EmployeeCommands.ToggleEmployeeStatus;
using SportAcademy.Application.Commands.EmployeeCommands.UpdateEmployee;
using SportAcademy.Application.Common.Localization;
using SportAcademy.Application.Common.Pagination;
using SportAcademy.Application.Queries.EmployeeQueries.GetActiveCoaches;
using SportAcademy.Application.Queries.EmployeeQueries.GetActiveCoachesCount;
using SportAcademy.Application.Queries.EmployeeQueries.GetActiveEmployees;
using SportAcademy.Application.Queries.EmployeeQueries.GetActiveEmployeesCount;
using SportAcademy.Application.Queries.EmployeeQueries.GetAll;
using SportAcademy.Application.Queries.EmployeeQueries.GetAllCoachs;
using SportAcademy.Application.Queries.EmployeeQueries.GetById;
using SportAcademy.Application.Queries.EmployeeQueries.GetCoachEmployeesWithoutCoachRecord;
using SportAcademy.Application.Queries.EmployeeQueries.ExportEmployees;
using SportAcademy.Application.Queries.EmployeeQueries.GetEmployeeImportTemplate;
using SportAcademy.Application.Queries.EmployeeQueries.GetEmployeesCount;
using SportAcademy.Application.Queries.EmployeeQueries.SearchEmployeess;
using SportAcademy.Application.Common.Result;
using SportAcademy.Domain.Enums;
using SportAcademy.Web.Features.Import;

namespace SportAcademy.Web.Controllers
{
[Authorize]
[EnableRateLimiting("per-user")]
[ApiController]
    [Route("api/[controller]")]
    public class EmployeeController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly ILocalizationService _localizer;

        public EmployeeController(IMediator mediator, ILocalizationService localizer)
        {
            _mediator = mediator;
            _localizer = localizer;
        }

        [HttpGet]
        [Authorize(Policy = "Permission:employee.manage")]
        public async Task<ActionResult> Index(
            [FromQuery] int? page,
            [FromQuery] int? pageSize,
            [FromQuery] string? status,
            [FromQuery] int? branchId,
            [FromQuery] string? position,
            [FromQuery] string? sortBy,
            [FromQuery] string? sortOrder,
            CancellationToken ct)
        {
            var result = await _mediator.Send(new GetAllEmployeesQuery(
                                                PageRequest.Create(page, pageSize),
                                                status,
                                                branchId,
                                                position,
                                                sortBy,
                                                sortOrder), ct);
            return Ok(result);
        }

        [HttpGet("{id}")]
        [Authorize(Policy = "Permission:employee.manage")]
        public async Task<ActionResult> Details(int id, CancellationToken ct)
        {
            var result = await _mediator.Send(new GetEmployeeByIdQuery(id), ct);
            return Ok(result);
        }

        [HttpPost]
        [Authorize(Policy = "Permission:employee.manage")]
        public async Task<ActionResult> CreateAsync(CreateEmployeeCommand command, CancellationToken ct)
        {
            var result = await _mediator.Send(command, ct);
            return Ok(result);
        }

        [HttpPut("{id}")]
        [Authorize(Policy = "Permission:employee.manage")]
        public async Task<ActionResult> EditAsync(int id, UpdateEmployeeCommand command, CancellationToken ct)
        {
            var result = await _mediator.Send(command with { Id = id }, ct);
            return Ok(result);
        }

        // Separate from PUT {id}: moving an employee out of Coach hard-deletes their coach record
        // (refused with an explanatory message when they still have groups/training history).
        [HttpPut("{id}/position")]
        [Authorize(Policy = "Permission:employee.manage")]
        public async Task<ActionResult> ChangePositionAsync(int id, ChangeEmployeePositionCommand command, CancellationToken ct)
        {
            var result = await _mediator.Send(command with { Id = id }, ct);
            return Ok(result);
        }

        [HttpDelete("{id}")]
        [Authorize(Policy = "Permission:employee.manage")]
        public async Task<ActionResult> Delete(int id, CancellationToken ct)
        {
            var result = await _mediator.Send(new DeleteEmployeeCommand(id), ct);
            return Ok(result);
        }

        [HttpPatch("{id}/toggle-status")]
        [Authorize(Policy = "Permission:employee.manage")]
        public async Task<IActionResult> ToggleStatus(int id, CancellationToken ct)
        {
            var result = await _mediator.Send(new ToggleEmployeeStatusCommand(id), ct);
            return Ok(result);
        }

        [HttpGet("active")]
        [Authorize(Policy = "Permission:employee.manage")]
        public async Task<IActionResult> GetActiveEmployees(
            [FromQuery] int? page,
            [FromQuery] int? pageSize,
            CancellationToken ct)
        {
            var result = await _mediator.Send(new GetActiveEmployeesQuery(
                                        PageRequest.Create(page, pageSize)), ct);
            return Ok(result);
        }

        [HttpGet("count")]
        public async Task<IActionResult> GetEmployeesCount(CancellationToken ct)
        {
            var result = await _mediator.Send(new GetEmployeesCountQuery(), ct);
            return Ok(result);
        }

        [HttpGet("active/count")]
        public async Task<IActionResult> GetActiveEmployeesCount(CancellationToken ct)
        {
            var result = await _mediator.Send(new GetActiveEmployeesCountQuery(), ct);
            return Ok(result);
        }

        [HttpGet("coaches/active")]
        [Authorize(Policy = "Permission:employee.manage")]
        public async Task<IActionResult> GetActiveCoaches(
            [FromQuery] int? page,
            [FromQuery] int? pageSize,
            CancellationToken ct)
        {
            var result = await _mediator.Send(new GetActiveCoachesQuery(
                                        PageRequest.Create(page, pageSize)), ct);
            return Ok(result);
        }

        [HttpGet("coaches/active/count")]
        public async Task<IActionResult> GetActiveCoachesCount(CancellationToken ct)
        {
            var result = await _mediator.Send(new GetActiveCoachesCountQuery(), ct);
            return Ok(result);
        }

        [HttpGet("coaches/employee")]
        [Authorize(Policy = "Permission:employee.manage")]
        public async Task<IActionResult> GetCoachEmployeesWithoutCoachRecord(
            [FromQuery] int? page,
            [FromQuery] int? pageSize,
            CancellationToken ct)
        {
            var result = await _mediator.Send(new GetCoachEmployeesWithoutCoachRecordQuery(
                                        PageRequest.Create(page, pageSize)), ct);
            return Ok(result);
        }

        [HttpGet("search")]
        [Authorize(Policy = "Permission:employee.manage")]
        public async Task<IActionResult> SearchEmployees(
            [FromQuery] string searchTerm,
            [FromQuery] int? page,
            [FromQuery] int? pageSize,
            CancellationToken ct)
        {
            var result = await _mediator.Send(new SearchEmployeeQuery(
                                        searchTerm, PageRequest.Create(page, pageSize)), ct);
            return Ok(result);
        }

        [HttpGet("positions")]
        public IActionResult GetPositions()
        {
            // value stays the English token the DB and raw SQL match on; only label is localized.
            var options = _localizer.Options<Position>();
            return Ok(Result<object>.Success(options, "GetAllPositions"));
        }

        [HttpGet("nationalities")]
        public IActionResult GetNationalities()
        {
            // Same shape as GetPositions - value stays the English enum token
            // (CreateEmployeeCommandHandler does Enum.Parse<Nationality>(request.Nationality)),
            // only the label shown in the dropdown is localized.
            var options = _localizer.Options<Nationality>();
            return Ok(Result<object>.Success(options, "GetAllNationalities"));
        }

        [HttpGet("coaches")]
        [Authorize(Policy = "Permission:employee.manage")]
        public async Task<IActionResult> GetAllCoaches(
            [FromQuery] int? page,
            [FromQuery] int? pageSize,
            [FromQuery] int? sportId,
            [FromQuery] int? branchId,
            CancellationToken ct)
        {
            var result = await _mediator.Send(new GetAllCoachsQuery(
                                        PageRequest.Create(page, pageSize), sportId, branchId), ct);
            return Ok(result);
        }

        // ── CSV import / export ───────────────────────────────────────────────────────────
        // Same two-phase flow as the trainee import: a dry run that reports every problem by row
        // and column, then an import of the rows that passed (re-checked first).

        // What the import screen shows (columns, required/optional, accepted values - this
        // academy's own branch names) and what the template is built from.
        [Authorize(Policy = "Permission:employee.manage")]
        [HttpGet("import/template")]
        public async Task<ActionResult> GetImportTemplate(CancellationToken ct)
        {
            var result = await _mediator.Send(new GetEmployeeImportTemplateQuery(), ct);
            return Ok(result);
        }

        // The same, as a ready-to-fill CSV: localized headers + two sample rows that import as-is.
        [Authorize(Policy = "Permission:employee.manage")]
        [HttpGet("import/template.csv")]
        public async Task<ActionResult> DownloadImportTemplate(CancellationToken ct)
        {
            var result = await _mediator.Send(new GetEmployeeImportTemplateQuery(), ct);
            if (!result.IsSuccess || result.Data is null)
                return Ok(result);

            var columns = result.Data.Columns;
            var bytes = CsvImportFile.Write(
                columns.Select(c => c.Label).ToList(),
                result.Data.SampleRows.Select(r => (IReadOnlyList<string?>)columns
                    .Select(c => r.TryGetValue(c.Key, out var v) ? v : null).ToList()));

            return File(bytes, "text/csv; charset=utf-8", "employees-import-template.csv");
        }

        // Dry run: every row checked, nothing saved.
        [Authorize(Policy = "Permission:employee.manage")]
        [HttpPost("import/validate")]
        [RequestSizeLimit(10 * 1024 * 1024)]
        public async Task<ActionResult> ValidateImport(IFormFile file, CancellationToken ct)
        {
            var (error, parsed) = await CsvImportFile.TryReadAsync(file, _localizer, ct);
            if (error is not null) return BadRequest(Result.Failure("Import", error, 400));

            var result = await _mediator.Send(new ValidateEmployeeImportCommand(parsed!.Headers, parsed.Rows), ct);
            return Ok(result);
        }

        // Re-validates, then saves the valid rows (validRowsOnly=true) or nothing unless every
        // row is valid (validRowsOnly=false).
        [Authorize(Policy = "Permission:employee.manage")]
        [HttpPost("import")]
        [RequestSizeLimit(10 * 1024 * 1024)]
        public async Task<ActionResult> ImportCsv(IFormFile file, [FromQuery] bool validRowsOnly = true, CancellationToken ct = default)
        {
            var (error, parsed) = await CsvImportFile.TryReadAsync(file, _localizer, ct);
            if (error is not null) return BadRequest(Result.Failure("Import", error, 400));

            var result = await _mediator.Send(new ImportEmployeesCommand(parsed!.Headers, parsed.Rows, validRowsOnly), ct);
            return Ok(result);
        }

        // The rows the console turns into a CSV. No ids = every employee the caller can see.
        [Authorize(Policy = "Permission:employee.manage")]
        [HttpPost("export")]
        public async Task<ActionResult> Export([FromBody] ExportEmployeesRequest? request, CancellationToken ct)
        {
            var result = await _mediator.Send(new ExportEmployeesQuery(request?.Ids), ct);
            return Ok(result);
        }
    }

    public record ExportEmployeesRequest(List<int>? Ids);
}
