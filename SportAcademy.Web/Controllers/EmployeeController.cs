using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SportAcademy.Application.Commands.EmployeeCommands.CreateEmployee;
using SportAcademy.Application.Commands.Trainees.CreateTrainee;
using SportAcademy.Application.Commands.Trainees.UpdateTrainee;
using SportAcademy.Application.Queries.EmployeeQueries.GetAll;
using SportAcademy.Application.Queries.EmployeeQueries.GetById;
using SportAcademy.Application.Queries.TraineeQueries.GetAll;
using SportAcademy.Application.Queries.TraineeQueries.GetById;

namespace SportAcademy.Web.Controllers
{
    //[Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class EmplopyeeController : Controller
    {
        private readonly IMediator _mediator;

        public EmplopyeeController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet("get-all")]
        public async Task<ActionResult> Index()
        {
            var employees = await _mediator.Send(new GetAllEmployeesQuery());
            return Ok(employees);
        }

        [HttpGet("get/{id}")]
        public async Task<ActionResult> Details(int id)
        {
            var trainee = await _mediator.Send(new GetEmployeeByIdQuery(id));
            return Ok(trainee);
        }

        [HttpPost("create")]
        public async Task<ActionResult> CreateAsync(CreateEmployeeCommand command)
        {
            var trainee = await _mediator.Send(command);
            return Ok(trainee);
        }

        [HttpPut("update")]
        public async Task<ActionResult> EditAsync(UpdateEmployeePersonalCommand command)
        {
            var employee = await _mediator.Send(command);
            return Ok(employee);
        }

        [HttpDelete("delete")]
        public ActionResult Delete(int id)
        {
            return NoContent();
        }
    }
}
