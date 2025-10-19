using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SportAcademy.Application.Commands.SubscriptionDetailsCommands.CreateSubscriptionDetails;

namespace SportAcademy.Web.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SubscriptionDetailsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public SubscriptionDetailsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost("create")]
        public async Task<IActionResult> Create([FromBody] CreateSubscriptionDetailsCommand command)
        {
            var result = await _mediator.Send(command);
            return Ok(result);
        }
    }
}
