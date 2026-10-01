using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SportAcademy.Application.Commands.Trainees.CreateTrainee;
using SportAcademy.Application.Commands.Trainees.DeleteTrainee;
using SportAcademy.Application.Commands.Trainees.ImportTrainees;
using SportAcademy.Application.Commands.Trainees.UpdateTraineeAcademicInfo;
using SportAcademy.Application.Commands.Trainees.UpdateTraineePersonalInfo;
using SportAcademy.Application.Common.Localization;
using SportAcademy.Application.Common.Pagination;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.Queries.TraineeQueries.GetImportTemplate;
using SportAcademy.Application.Queries.CoachQueries.GetCoachsCount;
using SportAcademy.Application.Queries.TraineeQueries.ExportTrainees;
using SportAcademy.Application.Queries.TraineeQueries.GetActiveTraineesCount;
using SportAcademy.Application.Queries.TraineeQueries.GetAll;
using SportAcademy.Application.Queries.TraineeQueries.GetAllForDropdown;
using SportAcademy.Application.Queries.TraineeQueries.GetAllTraineesOfSpecificDay;
using SportAcademy.Application.Queries.TraineeQueries.GetById;
using SportAcademy.Application.Queries.TraineeQueries.GetCoachHistory;
using SportAcademy.Application.Queries.TraineeQueries.GetSkillProgress;
using SportAcademy.Application.Queries.TraineeQueries.GetSportsSkill;
using SportAcademy.Application.Queries.TraineeQueries.GetTraineesCount;
using SportAcademy.Application.Queries.TraineeQueries.GetTraineesCountOfSpecificDay;
using SportAcademy.Application.Queries.TraineeQueries.SearchTrainee;
using SportAcademy.Application.Queries.TraineeQueries.SearchTraineeById;
using SportAcademy.Domain.Enums;
using SportAcademy.Web.Features.Import;
using SportAcademy.Web.Features.Trainees;
using SportAcademy.Web.Features.Trainees.Mappings;
using SportAcademy.Web.Features.Trainees.Requests;
using System.Globalization;
using System.Threading.Tasks;

namespace SportAcademy.Web.Controllers
{
[Authorize]
[EnableRateLimiting("per-user")]
[ApiController]
    [Route("api/[controller]")]
    public class TraineeController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly ILocalizationService _localizer;

        public TraineeController(IMediator mediator, ILocalizationService localizer)
        {
            _mediator = mediator;
            _localizer = localizer;
        }

        [HttpGet]
        public async Task<ActionResult> Index(
            [FromQuery] int? page,
            [FromQuery] int? pageSize,
            [FromQuery] TraineeFilter? filter,
            CancellationToken ct)
        {
            var trainees = await _mediator.Send(new GetAllTraineesQuery(
                        PageRequest.Create(page, pageSize),
                        filter?.SportId,
                        filter?.Status,
                        filter?.SortBy,
                        filter?.SortDir), ct);
            return Ok(trainees);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult> Details(int id)
        {
            var trainee = await _mediator.Send(new GetTraineeByIdQuery(id));
            return Ok(trainee);
        }

        [HttpGet("{id}/sports-skill")]
        public async Task<IActionResult> GetSportsSkill(int id, CancellationToken ct)
        {
            var result = await _mediator.Send(new GetSportsSkillQuery(id), ct);
            return Ok(result);
        }

        [HttpGet("{id}/skill-progress")]
        public async Task<IActionResult> GetSkillProgress(int id, CancellationToken ct)
        {
            var result = await _mediator.Send(new GetTraineeSkillProgressQuery(id), ct);
            return Ok(result);
        }

        [HttpGet("{id}/coaches")]
        public async Task<IActionResult> GetCoachHistory(int id, CancellationToken ct)
        {
            var result = await _mediator.Send(new GetTraineeCoachHistoryQuery(id), ct);
            return Ok(result);
        }

        [HttpGet("dropdown")]
        public async Task<IActionResult> GetDropdown(CancellationToken ct)
        {
            var result = await _mediator.Send(new GetAllTraineesForDropdownQuery(), ct);
            return Ok(result);
        }

        [Authorize(Policy = "Permission:trainee.register")]
        [HttpPost]
        public async Task<ActionResult> CreateAsync(CreateTraineeRequest request,
            CancellationToken ct)
        {
            var command = request.ToCommand();
            var trainee = await _mediator.Send(command, ct);

            if (!trainee.IsSuccess)
                return BadRequest(trainee);

            return Ok(trainee);
        }

        [Authorize(Policy = "Permission:trainee.edit")]
        [HttpPut("{id}/personal-info")]
        public async Task<ActionResult> EditPersonalInfoAsync(int id, UpdateTraineePersonalInfoRequest request, CancellationToken ct)
        {
            var command = request.ToCommand(id);
            var result = await _mediator.Send(command, ct);
            return Ok(result);
        }

        [Authorize(Policy = "Permission:trainee.edit")]
        [HttpPut("{id}/academic-info")]
        public async Task<ActionResult> EditAcademicInfoAsync(int id, UpdateTraineeAcademicInfoRequest request, CancellationToken ct)
        {
            var command = request.ToCommand(id);
            var result = await _mediator.Send(command, ct);
            return Ok(result);
        }

        [Authorize(Policy = "Permission:trainee.delete")]
        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(int id, CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(new DeleteTraineeCommand(id), cancellationToken);
            return Ok(result);
        }

        // What the import screen shows (columns, required/optional, accepted values - this
        // academy's own branch/category/sport names) and what the template is built from.
        [Authorize(Policy = "Permission:trainee.register")]
        [HttpGet("import/template")]
        public async Task<ActionResult> GetImportTemplate(CancellationToken ct)
        {
            var result = await _mediator.Send(new GetTraineeImportTemplateQuery(), ct);
            return Ok(result);
        }

        // The same, as a ready-to-fill CSV: localized headers + two sample rows that import as-is.
        [Authorize(Policy = "Permission:trainee.register")]
        [HttpGet("import/template.csv")]
        public async Task<ActionResult> DownloadImportTemplate(CancellationToken ct)
        {
            var result = await _mediator.Send(new GetTraineeImportTemplateQuery(), ct);
            if (!result.IsSuccess || result.Data is null)
                return Ok(result);

            var columns = result.Data.Columns;
            var bytes = CsvImportFile.Write(
                columns.Select(c => c.Label).ToList(),
                result.Data.SampleRows.Select(r => (IReadOnlyList<string?>)columns
                    .Select(c => r.TryGetValue(c.Key, out var v) ? v : null).ToList()));

            return File(bytes, "text/csv; charset=utf-8", "trainees-import-template.csv");
        }

        // Dry run: every row checked, nothing saved.
        [Authorize(Policy = "Permission:trainee.register")]
        [HttpPost("import/validate")]
        [RequestSizeLimit(10 * 1024 * 1024)]
        public async Task<ActionResult> ValidateImport(IFormFile file, CancellationToken ct)
        {
            var (error, parsed) = await ReadImportFileAsync(file, ct);
            if (error is not null) return error;

            var result = await _mediator.Send(new ValidateTraineeImportCommand(parsed!.Headers, parsed.Rows), ct);
            return Ok(result);
        }

        // Re-validates, then saves the valid rows (validRowsOnly=true) or nothing unless every
        // row is valid (validRowsOnly=false).
        [Authorize(Policy = "Permission:trainee.register")]
        [HttpPost("import")]
        [RequestSizeLimit(10 * 1024 * 1024)]
        public async Task<ActionResult> ImportCsv(IFormFile file, [FromQuery] bool validRowsOnly = true, CancellationToken ct = default)
        {
            var (error, parsed) = await ReadImportFileAsync(file, ct);
            if (error is not null) return error;

            var result = await _mediator.Send(new ImportTraineesCommand(parsed!.Headers, parsed.Rows, validRowsOnly), ct);
            return Ok(result);
        }

        private async Task<(ActionResult? Error, CsvImportFile.ReadResult? Parsed)> ReadImportFileAsync(IFormFile? file, CancellationToken ct)
        {
            var (error, parsed) = await CsvImportFile.TryReadAsync(file, _localizer, ct);
            return error is null ? (null, parsed) : (BadRequest(Result.Failure("Import", error, 400)), null);
        }

        [HttpGet("for-specific-day")]
        public async Task<IActionResult> GetAllForSpecificDay(
            [FromQuery] DateTime date,
            [FromQuery] int? page,
            [FromQuery] int? pageSize,
            CancellationToken ct)
        {
            var result = await _mediator.Send(new GetAllTraineesOfSpecificDayQuery(
                    date,
                    PageRequest.Create(page, pageSize)), ct);
            return Ok(result);
        }

        [HttpGet("count/for-specific-day")]
        public async Task<IActionResult> GetCountForSpecificDay(
            [FromQuery] DateTime date,
            CancellationToken ct)
        {
            var result = await _mediator.Send(new GetTraineesCountOfSpecificDayQuery(date), ct);
            return Ok(result);
        }

        [HttpGet("count")]
        public async Task<IActionResult> GetAllTraineesCount()
        {
            var result = await _mediator.Send(new GetTraineesCountQuery());
            return Ok(result);
        }

        [HttpGet("count-active")]
        public async Task<IActionResult> GetActiveTraineesCount(CancellationToken ct)
        {
            var result = await _mediator.Send(new GetActiveTraineesCountQuery(), ct);
            return Ok(result);
        }

        [HttpGet("search")]
        public async Task<IActionResult> Search(
            [FromQuery] string searchTerm,
            [FromQuery] int? page,
            [FromQuery] int? pageSize,
            [FromQuery] TraineeFilter? filter,
            CancellationToken cancellationToken)
        {
            var pageRequest = PageRequest.Create(page, pageSize);
            var result = await _mediator.Send(new SearchTraineeQuery(
                    searchTerm, pageRequest, filter?.SportId, filter?.Status, filter?.SortBy, filter?.SortDir),
                cancellationToken);

            return Ok(result);
        }

        [HttpGet("search/{id}")]
        public async Task<IActionResult> SearchById(
            [FromRoute] int id,
            [FromQuery] int? page,
            [FromQuery] int? pageSize,
            CancellationToken cancellationToken)
        {
            var pageRequest = PageRequest.Create(page, pageSize);
            var result = await _mediator.Send(new SearchTraineeByIdQuery(id.ToString(), pageRequest),
                cancellationToken);

            return Ok(result);
        }

        [Authorize(Policy = "Permission:trainee.export")]
        [HttpPost("export")]
        public async Task<ActionResult> Export(
            ExportTraineesRequest request,
            CancellationToken ct)
        {
            var result = await _mediator.Send(
                new ExportTraineesQuery(request.Ids), ct);

            if (!result.IsSuccess)
                return BadRequest(result);

            return Ok(result);
        }
    }
}
