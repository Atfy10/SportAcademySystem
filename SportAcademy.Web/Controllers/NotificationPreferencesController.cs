using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SportAcademy.Application.Commands.NotificationPreferenceCommands.UpdateMyNotificationPreferences;
using SportAcademy.Application.DTOs.NotificationsDtos;
using SportAcademy.Application.Queries.NotificationPreferenceQueries.GetMyNotificationPreferences;

namespace SportAcademy.Web.Controllers
{
    [Authorize]
    [EnableRateLimiting("per-user")]
    [Route("api/notification-preferences")]
    [ApiController]
    public class NotificationPreferencesController : ControllerBase
    {
        private readonly IMediator _mediator;

        public NotificationPreferencesController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        public async Task<IActionResult> GetMine(CancellationToken ct)
        {
            var result = await _mediator.Send(new GetMyNotificationPreferencesQuery(), ct);
            return StatusCode(result.StatusCode, result);
        }

        [HttpPut]
        public async Task<IActionResult> UpdateMine(
            [FromBody] List<NotificationPreferenceUpdateDto> preferences, CancellationToken ct)
        {
            var result = await _mediator.Send(new UpdateMyNotificationPreferencesCommand(preferences), ct);
            return StatusCode(result.StatusCode, result);
        }
    }
}
