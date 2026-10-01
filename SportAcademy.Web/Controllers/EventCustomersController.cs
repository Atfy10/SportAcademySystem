using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SportAcademy.Application.Commands.EventCustomerCommands.CreateEventCustomer;
using SportAcademy.Application.Commands.EventCustomerCommands.DeleteEventCustomer;
using SportAcademy.Application.Commands.EventCustomerCommands.UpdateEventCustomer;
using SportAcademy.Application.Common.Pagination;
using SportAcademy.Application.Queries.EventCustomerQueries.GetEventCustomerById;
using SportAcademy.Application.Queries.EventCustomerQueries.GetEventCustomerByPhone;
using SportAcademy.Application.Queries.EventCustomerQueries.GetEventCustomers;

namespace SportAcademy.Web.Controllers
{
    // The people who rent the academy for events - one record per phone number, reused across
    // bookings. Same feature gate and permissions as EventsController.
    [Authorize]
    [EnableRateLimiting("per-user")]
    [Route("api/event-customers")]
    [ApiController]
    public class EventCustomersController : ControllerBase
    {
        private readonly IMediator _mediator;

        public EventCustomersController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [Authorize(Policy = "Permission:event.view")]
        [HttpGet]
        public async Task<IActionResult> GetAll(
            [FromQuery] string? term, [FromQuery] int? nationalityCategoryId, [FromQuery] bool? isActive,
            [FromQuery] int? page, [FromQuery] int? pageSize, CancellationToken ct)
        {
            var result = await _mediator.Send(new GetEventCustomersQuery(
                PageRequest.Create(page, pageSize), term, nationalityCategoryId, isActive), ct);
            return Ok(result);
        }

        // Data is null (still a success) when nobody has this number yet.
        [Authorize(Policy = "Permission:event.view")]
        [HttpGet("by-phone")]
        public async Task<IActionResult> GetByPhone([FromQuery] string phone, CancellationToken ct)
        {
            var result = await _mediator.Send(new GetEventCustomerByPhoneQuery(phone), ct);
            return Ok(result);
        }

        [Authorize(Policy = "Permission:event.view")]
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id, CancellationToken ct)
        {
            var result = await _mediator.Send(new GetEventCustomerByIdQuery(id), ct);
            return Ok(result);
        }

        [Authorize(Policy = "Permission:event.manage")]
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateEventCustomerCommand command, CancellationToken ct)
        {
            var result = await _mediator.Send(command, ct);
            return Ok(result);
        }

        [Authorize(Policy = "Permission:event.manage")]
        [HttpPut]
        public async Task<IActionResult> Update([FromBody] UpdateEventCustomerCommand command, CancellationToken ct)
        {
            var result = await _mediator.Send(command, ct);
            return Ok(result);
        }

        [Authorize(Policy = "Permission:event.manage")]
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id, CancellationToken ct)
        {
            var result = await _mediator.Send(new DeleteEventCustomerCommand(id), ct);
            return Ok(result);
        }
    }
}
