using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SportAcademy.Application.Commands.Trainees.CreateTrainee;
using SportAcademy.Application.Commands.Trainees.UpdateTrainee;

namespace SportAcademy.Web.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TraineeController : Controller
    {
        private readonly IMediator _mediator;

        public TraineeController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        public IActionResult GetHello()
        {
            return Ok(new { Message = "Hello from Swagger!" });
        }

        [HttpGet("get-all")]
        public async Task<ActionResult> IndexAsync()
        {
            //await _mediator.Send();
            return Ok();
        }

        [HttpGet("get/{id}")]
        public ActionResult Details(int id)
        {
            return NoContent();
        }

        [HttpPost("create")]
        public async Task<ActionResult> CreateAsync(CreateTraineeCommand command)
        {
            var trainee = await _mediator.Send(command);
            return Ok(trainee);
        }

        [HttpPut]
        public async Task<ActionResult> EditAsync(UpdateTraineePersonalCommand command)
        {
            var trainee = await _mediator.Send(command);
            return Ok(trainee);
        }

        [HttpDelete]
        public ActionResult Delete(int id)
        {
            return NoContent();
        }
    }
}
