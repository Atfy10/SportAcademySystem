using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SportAcademy.Application.Commands.EmployeeCommands.CreateEmployee;
using SportAcademy.Application.Commands.EmployeeCommands.DeleteEmployee;
using SportAcademy.Application.Commands.EmployeeCommands.UpdateEmployee;
using SportAcademy.Application.Commands.Trainees.CreateTrainee;
using SportAcademy.Application.Commands.Trainees.UpdateTrainee;
using SportAcademy.Application.Queries.EmployeeQueries.GetAll;
using SportAcademy.Application.Queries.EmployeeQueries.GetById;
using SportAcademy.Application.Queries.TraineeQueries.GetAll;
using SportAcademy.Application.Queries.TraineeQueries.GetById;
using System.Threading.Tasks;

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
            var result = await _mediator.Send(new GetAllEmployeesQuery());
            return Ok(result);
        }

        [HttpGet("get/{id}")]
        public async Task<ActionResult> Details(int id)
        {
            var result = await _mediator.Send(new GetEmployeeByIdQuery(id));
            return Ok(result);
        }

        [HttpPost("create")]
        public async Task<ActionResult> CreateAsync(CreateEmployeeCommand command)
        {
            var result = await _mediator.Send(command);
            return Ok(result);
        }

        [HttpPut("update")]
        public async Task<ActionResult> EditAsync(UpdateEmployeeCommand command)
        {
            var result = await _mediator.Send(command);
            return Ok(result);
        }

        [HttpDelete("delete")]
        public async Task<ActionResult> Delete(int id)
        {
            var result = await _mediator.Send(new DeleteEmployeeCommand(id));
            return Ok(result);
        }
    }
}
