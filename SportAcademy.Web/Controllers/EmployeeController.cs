using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SportAcademy.Application.Commands.EmployeeCommands.CreateEmployee;
using SportAcademy.Application.Commands.EmployeeCommands.DeleteEmployee;
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

namespace SportAcademy.Web.Controllers
{
    [Authorize]
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
        public async Task<ActionResult> Index(
            [FromQuery] int? page,
            [FromQuery] int? pageSize,
            CancellationToken ct)
        {
            var result = await _mediator.Send(new GetAllEmployeesQuery(
                                                PageRequest.Create(page, pageSize)), ct);
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult> Details(int id, CancellationToken ct)
        {
            var result = await _mediator.Send(new GetEmployeeByIdQuery(id), ct);
            return Ok(result);
        }

        [HttpPost]
        public async Task<ActionResult> CreateAsync(CreateEmployeeCommand command, CancellationToken ct)
        {
            var result = await _mediator.Send(command, ct);
            return Ok(result);
        }

        [HttpPut("update")]
        public async Task<ActionResult> EditAsync(UpdateEmployeeCommand command, CancellationToken ct)
        {
            var result = await _mediator.Send(command, ct);
            return Ok(result);
        }

        [HttpDelete("delete")]
        public async Task<ActionResult> Delete(int id, CancellationToken ct)
        {
            var result = await _mediator.Send(new DeleteEmployeeCommand(id), ct);
            return Ok(result);
        }

        [HttpGet("active")]
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

        [HttpGet("coaches")]
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
