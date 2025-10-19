using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SportAcademy.Application.Commands.SubscriptionType.CreateSubscriptionType;

namespace SportAcademy.Web.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SubscriptionTypeController : ControllerBase
    {
        private readonly IMediator _mediator;

        public SubscriptionTypeController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost("create")]
        public async Task<IActionResult> Add(CreateSubscriptionTypeCommand command, CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(command, cancellationToken);
            return Ok(result);
        }
    }
}
