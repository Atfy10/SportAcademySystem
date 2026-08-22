using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SportAcademy.Application.Commands.EmployeeCommands.CreateEmployee;
using SportAcademy.Application.Commands.EmployeeCommands.DeleteEmployee;
using SportAcademy.Application.Commands.EmployeeCommands.ToggleEmployeeStatus;
using SportAcademy.Application.Commands.EmployeeCommands.UpdateEmployee;
using SportAcademy.Application.Common.Pagination;
using SportAcademy.Application.Queries.EmployeeQueries.GetActiveCoaches;
using SportAcademy.Application.Queries.EmployeeQueries.GetActiveCoachesCount;
using SportAcademy.Application.Queries.EmployeeQueries.GetActiveEmployees;
using SportAcademy.Application.Queries.EmployeeQueries.GetActiveEmployeesCount;
using SportAcademy.Application.Queries.EmployeeQueries.GetAll;
using SportAcademy.Application.Queries.EmployeeQueries.GetAllCoachs;
using SportAcademy.Application.Queries.EmployeeQueries.GetById;
using SportAcademy.Application.Queries.EmployeeQueries.GetCoachEmployeesWithoutCoachRecord;
using SportAcademy.Application.Queries.EmployeeQueries.GetEmployeesCount;
using SportAcademy.Application.Queries.EmployeeQueries.SearchEmployeess;
using SportAcademy.Application.Common.Result;
using SportAcademy.Domain.Enums;

namespace SportAcademy.Web.Controllers
{
[Authorize]
[EnableRateLimiting("per-user")]
[ApiController]
    [Route("api/[controller]")]
    public class EmployeeController : ControllerBase
    {
        private readonly IMediator _mediator;

        public EmployeeController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        [Authorize(Roles = "Owner,Admin")]
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
        [Authorize(Roles = "Owner,Admin")]
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

        [HttpDelete("{id}")]
        [Authorize(Roles = "Owner,Admin")]
        public async Task<ActionResult> Delete(int id, CancellationToken ct)
        {
            var result = await _mediator.Send(new DeleteEmployeeCommand(id), ct);
            return Ok(result);
        }

        [HttpPatch("{id}/toggle-status")]
        [Authorize(Roles = "Owner,Admin")]
        public async Task<IActionResult> ToggleStatus(int id, CancellationToken ct)
        {
            var result = await _mediator.Send(new ToggleEmployeeStatusCommand(id), ct);
            return Ok(result);
        }

        [HttpGet("active")]
        [Authorize(Roles = "Owner,Admin")]
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
        [Authorize(Roles = "Owner,Admin")]
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
        [Authorize(Roles = "Owner,Admin")]
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
        [Authorize(Roles = "Owner,Admin")]
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
            var options = Enum.GetValues<Position>()
                .Select(p => new { value = p, label = p.ToString() })
                .ToList();
            return Ok(Result<object>.Success(options, "GetAllPositions"));
        }

        [HttpGet("coaches")]
        [Authorize(Roles = "Owner,Admin")]
        public async Task<IActionResult> GetAllCoaches(
            [FromQuery] int? page,
            [FromQuery] int? pageSize,
            CancellationToken ct)
        {
            var result = await _mediator.Send(new GetAllCoachsQuery(
                                        PageRequest.Create(page, pageSize)), ct);
            return Ok(result);
        }
    }
}
