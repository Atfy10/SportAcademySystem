using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SportAcademy.Application.Common.Pagination;
using SportAcademy.Application.Common.Result;
using SportAcademy.Application.DTOs.NotificationsDtos;
using SportAcademy.Application.Interfaces;

namespace SportAcademy.Web.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class NotificationsController : ControllerBase
    {
        private readonly INotificationRepository _notificationRepository;
        private readonly IUserContextService _userContext;

        public NotificationsController(
            INotificationRepository notificationRepository,
            IUserContextService userContext)
        {
            _notificationRepository = notificationRepository;
            _userContext = userContext;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll(
            [FromQuery] int? page,
            [FromQuery] int? pageSize,
            CancellationToken ct)
        {
            var result = await _notificationRepository.GetUserNotificationsAsync(
                _userContext.UserId,
                PageRequest.Create(page, pageSize),
                ct);

            return Ok(Result<PagedData<NotificationRecipientDto>>.Success(result, "Get"));
        }

        [HttpGet("unread-count")]
        public async Task<IActionResult> GetUnreadCount(CancellationToken ct)
        {
            var count = await _notificationRepository.GetUnreadCountAsync(_userContext.UserId, ct);
            return Ok(Result<int>.Success(count, "Get"));
        }

        [HttpPatch("{id}/read")]
        public async Task<IActionResult> MarkAsRead(int id, CancellationToken ct)
        {
            var success = await _notificationRepository.MarkAsReadAsync(id, _userContext.UserId, ct);

            if (!success)
                return NotFound(Result.Failure("Update", "Notification not found"));

            return Ok(Result<object?>.Success(null, "Update"));
        }

        [HttpPatch("read-all")]
        public async Task<IActionResult> MarkAllAsRead(CancellationToken ct)
        {
            await _notificationRepository.MarkAllAsReadAsync(_userContext.UserId, ct);
            return Ok(Result<object?>.Success(null, "Update"));
        }
    }
}
