using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SportAcademy.Application.Commands.PaymentTypeCommands.CreatePaymentType;
using SportAcademy.Application.Commands.PaymentTypeCommands.DeletePaymentType;
using SportAcademy.Application.Commands.PaymentTypeCommands.UpdatePaymentType;
using SportAcademy.Application.Queries.PaymentTypeQueries.GetAll;
using SportAcademy.Application.Queries.PaymentTypeQueries.GetById;

namespace SportAcademy.Web.Controllers
{
    [Authorize]
    [EnableRateLimiting("per-user")]
    [Route("api/[controller]")]
    [ApiController]
    public class PaymentTypesController : ControllerBase
    {
        private readonly IMediator _mediator;

        public PaymentTypesController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var result = await _mediator.Send(new GetAllPaymentTypesQuery());
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Details(int id)
        {
            var result = await _mediator.Send(new GetPaymentTypeByIdQuery(id));
            return Ok(result);
        }

        [Authorize(Policy = "Permission:paymenttype.manage")]
        [HttpPost]
        public async Task<IActionResult> CreateAsync(CreatePaymentTypeCommand command)
        {
            var result = await _mediator.Send(command);
            return Ok(result);
        }

        [Authorize(Policy = "Permission:paymenttype.manage")]
        [HttpPut]
        public async Task<IActionResult> EditAsync(UpdatePaymentTypeCommand command)
        {
            var result = await _mediator.Send(command);
            return Ok(result);
        }

        [Authorize(Policy = "Permission:paymenttype.manage")]
        [HttpDelete]
        public async Task<IActionResult> Delete(DeletePaymentTypeCommand command)
        {
            var result = await _mediator.Send(command);
            if (!result.IsSuccess)
                return BadRequest(result.Message);

            return NoContent();
        }
    }
}
