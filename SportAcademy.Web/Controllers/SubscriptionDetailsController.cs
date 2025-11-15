using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SportAcademy.Application.Commands.SubscriptionDetailsCommands.CreateSubscriptionDetails;
using SportAcademy.Application.Commands.SubscriptionDetailsCommands.DeleteSubscriptionDetails;
using SportAcademy.Application.Commands.SubscriptionDetailsCommands.UpdateSubscriptionDetails;
using SportAcademy.Application.Queries.SubscriptionDetailsQueries.GetAll;
using SportAcademy.Application.Queries.SubscriptionDetailsQueries.GetById;

namespace SportAcademy.Web.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SubscriptionDetailsController : ControllerBase
    {
        IMediator _mediator;

        public SubscriptionDetailsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet("get-all")]
        public async Task<IActionResult> Index()
        {
            var result = await _mediator.Send(new GetAllSubDetailsQuery());
            return Ok(result);
        }

        [HttpGet("get/{id}")]
        public async Task<IActionResult> Details(int id)
        {
            var result = await _mediator.Send(new GetSubDetailsByIdQuery(id));
            return Ok(result);
        }

        [HttpPost("create")]
        public async Task<IActionResult> CreateAsync(CreateSubscriptionDetailsCommand command)
        {
            var result = await _mediator.Send(command);
            return Ok(result);
        }

        [HttpPut("update")]
        public async Task<IActionResult> EditAsync(UpdateSubscriptionDetailsCommand command)
        {
            var result = await _mediator.Send(command);
            return Ok(result);
        }

        [HttpDelete("delete")]
        public IActionResult Delete(DeleteSubscriptionDetailsCommand command)
        {
            var result = _mediator.Send(command);
            if (result is null || !result.Result.IsSuccess)
                return BadRequest(result?.Result.Message);

            return NoContent();
        }
    }

}

