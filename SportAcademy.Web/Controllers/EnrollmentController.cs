using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SportAcademy.Application.Commands.EnrollmentCommands.CreateEnrollment;
using SportAcademy.Application.Commands.EnrollmentCommands.DeleteEnrollment;
using SportAcademy.Application.Commands.EnrollmentCommands.UpdateEnrollment;
using SportAcademy.Application.Common.Pagination;
using SportAcademy.Application.Queries.EnrollmentQueries.GetAll;
using SportAcademy.Application.Queries.EnrollmentQueries.GetAllEnrollmentsForAllSports;
using SportAcademy.Application.Queries.EnrollmentQueries.GetAllEnrollmentsForSport;
using SportAcademy.Application.Queries.EnrollmentQueries.GetById;
using SportAcademy.Application.Queries.EnrollmentQueries.GetEnrollmentsCountForSport;
using SportAcademy.Application.Queries.EnrollmentQueries.GetEnrollmentsCountForSports;

namespace SportAcademy.Web.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class EnrollmentController : ControllerBase
    {
        private readonly IMediator _mediator;

        public EnrollmentController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost("create")]
        public async Task<IActionResult> Create([FromBody] CreateEnrollmentCommand command, CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(command, cancellationToken);
            return Ok(result);
        }

        [HttpGet("get-all")]
        public async Task<IActionResult> GetAll()
        {
            var result = await _mediator.Send(new GetAllEnrollmentsQuery());
            return Ok(result);
        }

        [HttpGet("get/{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _mediator.Send(new GetEnrollmentByIdQuery(id));
            return Ok(result);
        }

        [HttpPut("update")]
        public async Task<IActionResult> Update([FromBody] UpdateEnrollmentCommand command,
            CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(command, cancellationToken);
            return Ok(result);
        }

        [HttpDelete("delete")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _mediator.Send(new DeleteEnrollmentCommand(id));
            return Ok(result);
        }

        [HttpGet("sports/enrollments")]
        public async Task<IActionResult> GetAllEnrollmentsForAllSports(
            [FromQuery] int? page,
            [FromQuery] int? pageSize,
            [FromQuery] DateTime? from,
            [FromQuery] DateTime? to,
            CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(new
                GetAllEnrollmentsForAllSportsQuery(from, to, PageRequest.Create(page, pageSize)),
                cancellationToken);

            return Ok(result);
        }

        [HttpGet("sports/enrollments/count")]
        public async Task<IActionResult> GetAllEnrollmentsCountForSports(
            [FromQuery] DateTime? from,
            [FromQuery] DateTime? to,
            CancellationToken ct)
        {
            var result = await _mediator.Send(new GetEnrollmentsCountForSportsQuery(from, to), ct);
            return Ok(result);
        }

        [HttpGet("sports/{sportId}/enrollments")]
        public async Task<IActionResult> GetAllEnrollmentsForSport(
            [FromRoute] int sportId,
            [FromQuery] int? page,
            [FromQuery] int? pageSize,
            [FromQuery] DateTime? from,
            [FromQuery] DateTime? to,
            CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(
                new GetAllEnrollmentsForSportQuery(sportId, from, to, PageRequest.Create(page, pageSize)),
                cancellationToken);
            return Ok(result);
        }

        [HttpGet("sports/{sportId}/enrollments/count")]
        public async Task<IActionResult> GetAllEnrollmentsCountForSport(
            [FromRoute] int sportId,
            [FromQuery] DateTime? from,
            [FromQuery] DateTime? to,
            CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(
                new GetEnrollmentsCountForSportQuery(sportId, from, to),
                cancellationToken);
            return Ok(result);
        }
    }
}
