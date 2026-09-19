using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SportAcademy.Application.Commands.NotificationRoutingCommands.UpdateTenantNotificationChannelRules;
using SportAcademy.Application.DTOs.NotificationsDtos;
using SportAcademy.Application.Queries.NotificationRoutingQueries.GetTenantNotificationMatrix;

namespace SportAcademy.Web.Controllers
{
    [Authorize]
    [EnableRateLimiting("per-user")]
    [Route("api/tenant/notification-routing")]
    [ApiController]
    public class NotificationRoutingController : ControllerBase
    {
        private readonly IMediator _mediator;

        public NotificationRoutingController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        public async Task<IActionResult> GetMatrix(CancellationToken ct)
        {
            var result = await _mediator.Send(new GetTenantNotificationMatrixQuery(), ct);
            return StatusCode(result.StatusCode, result);
        }

        [Authorize(Policy = "Permission:tenant.settings.manage")]
        [HttpPut]
        public async Task<IActionResult> UpdateMatrix(
            [FromBody] List<NotificationRuleUpdateDto> rules, CancellationToken ct)
        {
            var result = await _mediator.Send(new UpdateTenantNotificationChannelRulesCommand(rules), ct);
            return StatusCode(result.StatusCode, result);
        }
    }
}
