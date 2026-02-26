using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SportAcademy.Application.Commands.Trainees.CreateTrainee;
using SportAcademy.Application.Commands.Trainees.UpdateTrainee;
using SportAcademy.Application.Common.Pagination;
using SportAcademy.Application.Queries.TraineeQueries.GetAll;
using SportAcademy.Application.Queries.TraineeQueries.GetAllTraineesOfSpecificDay;
using SportAcademy.Application.Queries.TraineeQueries.GetById;
using SportAcademy.Application.Queries.TraineeQueries.GetTraineesCountOfSpecificDay;
using System.Threading.Tasks;

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

        [HttpGet("get-all")]
        public async Task<ActionResult> Index(
            [FromQuery] int? page,
            [FromQuery] int? pageSize,
            CancellationToken ct)
        {
            var trainees = await _mediator.Send(new GetAllTraineesQuery(
                                        PageRequest.Create(page, pageSize)), ct);
            return Ok(trainees);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult> Details(int id)
        {
            var trainee = await _mediator.Send(new GetTraineeByIdQuery(id));
            return Ok(trainee);
        }

        [HttpPost("create")]
        public async Task<ActionResult> CreateAsync(CreateTraineeCommand command)
        {
            var trainee = await _mediator.Send(command);
            return Ok(trainee);
        }

        [HttpPut("update")]
        public async Task<ActionResult> EditAsync(UpdateTraineePersonalCommand command)
        {
            var trainee = await _mediator.Send(command);
            return Ok(trainee);
        }

        [HttpDelete("delete")]
        public ActionResult Delete(int id)
        {
            return NoContent();
        }

        [HttpGet("get-all-for-specific-day")]
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

        [HttpGet("get-count-for-specific-day")]
        public async Task<IActionResult> GetCountForSpecificDay(
            [FromQuery] DateTime date,
            CancellationToken ct)
        {
            var result = await _mediator.Send(new GetTraineesCountOfSpecificDayQuery(date), ct);
            return Ok(result);
        }
    }
}
