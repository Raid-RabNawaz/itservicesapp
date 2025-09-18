using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ITServicesApp.Application.DTOs;
using ITServicesApp.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ITServicesApp.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public sealed class NotificationController : ControllerBase
    {
        private readonly INotificationService _notifications;

        public NotificationController(INotificationService notifications) => _notifications = notifications;

        [HttpGet]
        public async Task<ActionResult<IReadOnlyList<NotificationDto>>> List([FromQuery] int take = 20, [FromQuery] int skip = 0, CancellationToken ct = default)
            => Ok(await _notifications.ListAsync(null, take, skip, ct)); // null = current user in service

        [HttpGet("unread/count")]
        public async Task<ActionResult<int>> CountUnread(CancellationToken ct)
            => Ok(await _notifications.CountUnreadAsync(ct));

        [HttpPost("{id:int}/read")]
        public async Task<IActionResult> MarkRead(int id, CancellationToken ct)
        {
            await _notifications.MarkReadAsync(id, ct);
            return NoContent();
        }

        [HttpPost("read-all")]
        public async Task<IActionResult> MarkAllRead(CancellationToken ct)
        {
            await _notifications.MarkAllReadAsync(ct);
            return NoContent();
        }
    }
}
