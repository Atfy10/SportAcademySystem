using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using SportAcademy.Application.Commands.PushSubscriptionCommands.RegisterPushSubscription;
using SportAcademy.Application.Commands.PushSubscriptionCommands.UnregisterPushSubscription;
using SportAcademy.Infrastructure.Options;

namespace SportAcademy.Web.Controllers
{
    [Authorize]
    [EnableRateLimiting("per-user")]
    [Route("api/push-subscriptions")]
    [ApiController]
    public class PushSubscriptionsController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly WebPushSettings _webPushSettings;

        public PushSubscriptionsController(IMediator mediator, IOptions<WebPushSettings> webPushSettings)
        {
            _mediator = mediator;
            _webPushSettings = webPushSettings.Value;
        }

        // Plain config read, no MediatR - same precedent as CoachController.GetSkillLevels: not
        // business data, nothing to gate or audit, just a server-side value the browser needs to
        // call PushManager.subscribe({ applicationServerKey: ... }).
        [HttpGet("vapid-public-key")]
        public IActionResult GetVapidPublicKey()
            => Ok(new { publicKey = _webPushSettings.PublicKey });

        [HttpPost]
        public async Task<IActionResult> Register(
            [FromBody] RegisterPushSubscriptionCommand command, CancellationToken ct)
        {
            var result = await _mediator.Send(command, ct);
            return StatusCode(result.StatusCode, result);
        }

        [HttpDelete]
        public async Task<IActionResult> Unregister(
            [FromBody] UnregisterPushSubscriptionCommand command, CancellationToken ct)
        {
            var result = await _mediator.Send(command, ct);
            return StatusCode(result.StatusCode, result);
        }
    }
}
