using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SportAcademy.Application.Commands.EmployeeCommands.CreateEmployee;
using SportAcademy.Application.Commands.EmployeeCommands.DeleteEmployee;
using SportAcademy.Application.Commands.EmployeeCommands.UpdateEmployee;
using SportAcademy.Application.Commands.Trainees.CreateTrainee;
using SportAcademy.Application.Commands.Trainees.UpdateTrainee;
using SportAcademy.Application.Queries.EmployeeQueries.GetActiveCoaches;
using SportAcademy.Application.Queries.EmployeeQueries.GetActiveEmployees;
using SportAcademy.Application.Queries.EmployeeQueries.GetAll;
using SportAcademy.Application.Queries.EmployeeQueries.GetById;
using SportAcademy.Application.Queries.EmployeeQueries.GetCoachEmployeesWithoutCoachRecord;
using SportAcademy.Application.Queries.TraineeQueries.GetAll;
using SportAcademy.Application.Queries.TraineeQueries.GetById;
using System.Threading.Tasks;

namespace SportAcademy.Web.Controllers
{
    //[Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class EmplopyeeController : ControllerBase
    {
        private readonly IMediator _mediator;

        public EmplopyeeController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet("get-all")]
        public async Task<ActionResult> Index(CancellationToken ct)
        {
            var result = await _mediator.Send(new GetAllEmployeesQuery(), ct);
            return Ok(result);
        }

        [HttpGet("get/{id}")]
        public async Task<ActionResult> Details(int id, CancellationToken ct)
        {
            var result = await _mediator.Send(new GetEmployeeByIdQuery(id), ct);
            return Ok(result);
        }

        [HttpPost("create")]
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

        [HttpGet("get-active")]
        public async Task<IActionResult> GetActive(CancellationToken ct)
        {
            var result = await _mediator.Send(new GetActiveEmployeesQuery(), ct);
            return Ok(result);
        }

        [HttpGet("get-active-coaches")]
        public async Task<IActionResult> GetActiveCoaches(CancellationToken ct)
        {
            var result = await _mediator.Send(new GetActiveCoachesQuery(), ct);
            return Ok(result);
        }

        [HttpGet("get-coach-employees")]
        public async Task<IActionResult> GetCoachEmployeesWithoutCoachRecord(CancellationToken ct)
        {
            var result = await _mediator.Send(new GetCoachEmployeesWithoutCoachRecordQuery(), ct);
            return Ok(result);
        }
    }
}
