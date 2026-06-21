using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SportAcademy.Application.Commands.SubscriptionTypeCommands.CreateSubscriptionType;
using SportAcademy.Application.Commands.SubscriptionTypeCommands.DeleteSubscriptionType;
using SportAcademy.Application.Commands.SubscriptionTypeCommands.UpdateSubscriptionType;
using SportAcademy.Application.Queries.SubscriptionTypeQueries.GetAll;
using SportAcademy.Application.Queries.SubscriptionTypeQueries.GetById;

namespace SportAcademy.Web.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class SubscriptionTypesController : ControllerBase
    {
        private readonly IMediator _mediator;

        public SubscriptionTypesController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var result = await _mediator.Send(new GetAllSubscriptionTypesQuery());
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Details(int id)
        {
            var result = await _mediator.Send(new GetSubscriptionTypeByIdQuery(id));
            return Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> CreateAsync(CreateSubscriptionTypeCommand command)
        {
            var result = await _mediator.Send(command);
            return Ok(result);
        }

        [HttpPut]
        public async Task<IActionResult> EditAsync(UpdateSubscriptionTypeCommand command)
        {
            var result = await _mediator.Send(command);
            return Ok(result);
        }

        [HttpDelete]
        public IActionResult Delete(DeleteSubscriptionTypeCommand command)
        {
            var result = _mediator.Send(command);
            if (result is null || !result.Result.IsSuccess)
                return BadRequest(result?.Result.Message);

            return NoContent();
        }
    }
}