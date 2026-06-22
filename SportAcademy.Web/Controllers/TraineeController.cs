using CsvHelper;
using CsvHelper.Configuration;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SportAcademy.Application.Commands.Trainees.CreateTrainee;
using SportAcademy.Application.Commands.Trainees.DeleteTrainee;
using SportAcademy.Application.Commands.Trainees.ImportTrainees;
using SportAcademy.Application.Commands.Trainees.UpdateTrainee;
using SportAcademy.Application.Common.Pagination;
using SportAcademy.Application.Queries.CoachQueries.GetCoachsCount;
using SportAcademy.Application.Queries.TraineeQueries.GetActiveTraineesCount;
using SportAcademy.Application.Queries.TraineeQueries.GetAll;
using SportAcademy.Application.Queries.TraineeQueries.GetAllTraineesOfSpecificDay;
using SportAcademy.Application.Queries.TraineeQueries.GetById;
using SportAcademy.Application.Queries.TraineeQueries.GetTraineesCount;
using SportAcademy.Application.Queries.TraineeQueries.GetTraineesCountOfSpecificDay;
using SportAcademy.Application.Queries.TraineeQueries.GetAllForDropdown;
using SportAcademy.Application.Queries.TraineeQueries.SearchTrainee;
using SportAcademy.Application.Queries.TraineeQueries.SearchTraineeById;
using SportAcademy.Application.Queries.TraineeQueries.ExportTrainees;
using SportAcademy.Domain.Enums;
using System.Globalization;
using System.Threading.Tasks;
using SportAcademy.Web.Features.Trainees;
using SportAcademy.Web.Features.Trainees.Requests;
using SportAcademy.Web.Features.Trainees.Mappings;

namespace SportAcademy.Web.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class TraineeController : ControllerBase
    {
        private readonly IMediator _mediator;

        public TraineeController(IMediator mediator)
        {
            _mediator = mediator;
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
                        filter?.Sport,
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

        [HttpGet("dropdown")]
        public async Task<IActionResult> GetDropdown(CancellationToken ct)
        {
            var result = await _mediator.Send(new GetAllTraineesForDropdownQuery(), ct);
            return Ok(result);
        }

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

        [HttpPut("{id}")]
        public async Task<ActionResult> EditAsync(int id, UpdateTraineeRequest request, CancellationToken ct)
        {
            var command = request.ToCommand(id);
            var trainee = await _mediator.Send(command, ct);
            return Ok(trainee);
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(int id, CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(new DeleteTraineeCommand(id), cancellationToken);
            return Ok(result);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost("import")]
        [RequestSizeLimit(10 * 1024 * 1024)]
        public async Task<ActionResult> ImportCsv(IFormFile file, CancellationToken ct)
        {
            if (file == null || file.Length == 0)
                return BadRequest("File is required.");

            if (!file.FileName.EndsWith(".csv", StringComparison.OrdinalIgnoreCase))
                return BadRequest("Only .csv files are supported.");

            var config = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = true,
                MissingFieldFound = null,
                HeaderValidated = null,
                BadDataFound = null,
                TrimOptions = TrimOptions.Trim,
            };

            using var reader = new StreamReader(file.OpenReadStream());
            using var csv = new CsvReader(reader, config);

            csv.Context.RegisterClassMap<ImportTraineeCsvRowMap>();

            var records = csv.GetRecords<ImportTraineeCsvRow>().ToList();

            if (records.Count == 0)
                return BadRequest("CSV file is empty.");

            var commands = new List<CreateTraineeCommand>();

            foreach (var row in records)
            {
                var sportIds = new HashSet<int>();
                if (!string.IsNullOrWhiteSpace(row.SportIds))
                {
                    foreach (var id in row.SportIds.Split('|', StringSplitOptions.RemoveEmptyEntries))
                    {
                        if (int.TryParse(id.Trim(), out var sportId))
                            sportIds.Add(sportId);
                    }
                }

                var command = new CreateTraineeCommand
                {
                    FirstName = row.FirstName,
                    LastName = row.LastName,
                    SSN = row.SSN,
                    PhoneNumber = row.PhoneNumber,
                    Email = row.Email,
                    BirthDate = DateOnly.ParseExact(row.BirthDate, "yyyy-MM-dd", CultureInfo.InvariantCulture),
                    Gender = Enum.Parse<Gender>(row.Gender, ignoreCase: true),
                    Nationality = Enum.Parse<Nationality>(row.Nationality, ignoreCase: true),
                    BranchId = int.Parse(row.BranchId),
                    NationalityCategoryId = int.Parse(row.NationalityCategoryId),
                    SportIds = sportIds,
                    FamilyId = string.IsNullOrWhiteSpace(row.FamilyId) ? 0 : int.Parse(row.FamilyId),
                    ParentNumber = string.IsNullOrWhiteSpace(row.ParentNumber) ? null : row.ParentNumber,
                    GuardianName = string.IsNullOrWhiteSpace(row.GuardianName) ? null : row.GuardianName,
                    Street = string.IsNullOrWhiteSpace(row.Street) ? null : row.Street,
                    City = string.IsNullOrWhiteSpace(row.City) ? null : row.City,
                };

                commands.Add(command);
            }

            var importCommand = new ImportTraineesCommand(commands);
            var result = await _mediator.Send(importCommand, ct);

            if (!result.IsSuccess)
                return BadRequest(result);

            return Ok(result);
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
                    searchTerm, pageRequest, filter?.Sport, filter?.Status, filter?.SortBy, filter?.SortDir),
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
