using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SportAcademy.Application.Commands.Trainees.CreateTrainee;
using SportAcademy.Application.Commands.Trainees.DeleteTrainee;
using SportAcademy.Application.Commands.Trainees.UpdateTrainee;
using SportAcademy.Application.Common.Pagination;
using SportAcademy.Application.Queries.CoachQueries.GetCoachsCount;
using SportAcademy.Application.Queries.TraineeQueries.GetActiveTraineesCount;
using SportAcademy.Application.Queries.TraineeQueries.GetAll;
using SportAcademy.Application.Queries.TraineeQueries.GetAllTraineesOfSpecificDay;
using SportAcademy.Application.Queries.TraineeQueries.GetById;
using SportAcademy.Application.Queries.TraineeQueries.GetTraineesCount;
using SportAcademy.Application.Queries.TraineeQueries.GetTraineesCountOfSpecificDay;
using SportAcademy.Application.Queries.TraineeQueries.SearchTrainee;
using SportAcademy.Application.Queries.TraineeQueries.SearchTraineeById;
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

        [HttpGet]
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

        [HttpPost]
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

        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(int id, CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(new DeleteTraineeCommand(id), cancellationToken);
            return Ok(result);
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
            CancellationToken cancellationToken)
        {
            var pageRequest = PageRequest.Create(page, pageSize);
            var result = await _mediator.Send(new SearchTraineeQuery(searchTerm, pageRequest), 
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
    }
}
