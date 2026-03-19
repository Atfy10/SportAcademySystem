using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SportAcademy.Application.Commands.EnrollmentCommands.ActivateEnrollment;
using SportAcademy.Application.Commands.EnrollmentCommands.CreateEnrollment;
using SportAcademy.Application.Commands.EnrollmentCommands.DeleteEnrollment;
using SportAcademy.Application.Commands.EnrollmentCommands.SuspendEnrollment;
using SportAcademy.Application.Commands.EnrollmentCommands.UpdateEnrollment;
using SportAcademy.Application.Commands.EnrollmentCommands.UpdatePaymentStatus;
using SportAcademy.Application.Common.Pagination;
using SportAcademy.Application.Queries.EnrollmentQueries.CountActive;
using SportAcademy.Application.Queries.EnrollmentQueries.CountAll;
using SportAcademy.Application.Queries.EnrollmentQueries.CountPendingPayment;
using SportAcademy.Application.Queries.EnrollmentQueries.GetAll;
using SportAcademy.Application.Queries.EnrollmentQueries.GetAllEnrollmentsForAllSports;
using SportAcademy.Application.Queries.EnrollmentQueries.GetAllEnrollmentsForSport;
using SportAcademy.Application.Queries.EnrollmentQueries.GetById;
using SportAcademy.Application.Queries.EnrollmentQueries.GetEnrollmentsCountForSport;
using SportAcademy.Application.Queries.EnrollmentQueries.GetEnrollmentsCountForSports;
using SportAcademy.Application.Queries.EnrollmentQueries.SearchEnrollments;

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

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateEnrollmentCommand command, CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(command, cancellationToken);
            return Ok(result);
        }

        [HttpGet]
        public async Task<IActionResult> GetAll(
            [FromQuery] int? page,
            [FromQuery] int? pageSize,
            CancellationToken ct)
        {
            var result = await _mediator.Send(
                new GetAllEnrollmentsQuery(PageRequest.Create(page, pageSize)), ct);
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _mediator.Send(new GetEnrollmentByIdQuery(id));
            return Ok(result);
        }

        [HttpPut]
        public async Task<IActionResult> Update([FromBody] UpdateEnrollmentCommand command,
            CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(command, cancellationToken);
            return Ok(result);
        }

        [HttpDelete("{id}")]
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

        [HttpGet("search")]
        public async Task<IActionResult> Search(
            [FromQuery] string term,
            [FromQuery] int? page,
            [FromQuery] int? pageSize,
            CancellationToken ct)
        {
            var result = await _mediator.Send(
                new SearchEnrollmentsQuery(term, PageRequest.Create(page, pageSize)), ct);
            return Ok(result);
        }

        [HttpGet("count")]
        public async Task<IActionResult> CountAll(CancellationToken ct)
        {
            var result = await _mediator.Send(new CountAllEnrollmentsQuery(), ct);
            return Ok(result);
        }

        [HttpGet("count/active")]
        public async Task<IActionResult> CountActive(CancellationToken ct)
        {
            var result = await _mediator.Send(new CountActiveEnrollmentsQuery(), ct);
            return Ok(result);
        }

        [HttpGet("count/pending-payment")]
        public async Task<IActionResult> CountPendingPayment(CancellationToken ct)
        {
            var result = await _mediator.Send(new CountPendingPaymentsQuery(), ct);
            return Ok(result);
        }

        [HttpPatch("{id}/activate")]
        public async Task<IActionResult> Activate(
            [FromRoute] int id,
            CancellationToken ct)
        {
            var result = await _mediator.Send(new ActivateEnrollmentCommand(id), ct);
            return Ok(result);
        }

        [HttpPatch("{id}/suspend")]
        public async Task<IActionResult> Suspend(
            [FromRoute] int id,
            CancellationToken ct)
        {
            var result = await _mediator.Send(new SuspendEnrollmentCommand(id), ct);
            return Ok(result);
        }

        [HttpPatch("{id}/payment-status")]
        public async Task<IActionResult> UpdatePaymentStatus(
            [FromRoute] int id,
            [FromBody] UpdatePaymentStatusCommand command,
            CancellationToken ct)
        {
            var result = await _mediator.Send(
                new UpdatePaymentStatusCommand(id, command.PaymentStatus), ct);
            return Ok(result);
        }
    }
}
