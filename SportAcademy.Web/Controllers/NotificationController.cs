using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SportAcademy.Application.Commands.NotificationCommands.MarkAllNotificationsAsRead;
using SportAcademy.Application.Commands.NotificationCommands.MarkNotificationAsRead;
using SportAcademy.Application.Common.Pagination;
using SportAcademy.Application.Queries.NotificationQueries.GetUnreadCount;
using SportAcademy.Application.Queries.NotificationQueries.GetUserNotifications;

namespace SportAcademy.Web.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class NotificationsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public NotificationsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll(
            [FromQuery] int? page,
            [FromQuery] int? pageSize,
            CancellationToken ct)
        {
            var result = await _mediator.Send(
                new GetUserNotificationsQuery(PageRequest.Create(page, pageSize)),
                ct);

            return Ok(result);
        }

        [HttpGet("unread-count")]
        public async Task<IActionResult> GetUnreadCount(CancellationToken ct)
        {
            var count = await _mediator.Send(new GetUnreadCountQuery(), ct);
            return Ok(count);
        }

        [HttpPatch("{id}/read")]
        public async Task<IActionResult> MarkAsRead(int id, CancellationToken ct)
        {
            var success = await _mediator.Send(
                new MarkNotificationAsReadCommand(id),
                ct);

            if (!success)
                return NotFound();

            return NoContent();
        }

        [HttpPatch("read-all")]
        public async Task<IActionResult> MarkAllAsRead(CancellationToken ct)
        {
            await _mediator.Send(new MarkAllNotificationsAsReadCommand(), ct);
            return NoContent();
        }
    }
}
