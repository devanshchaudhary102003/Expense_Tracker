using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NotificationService.DTOs;
using NotificationService.Interfaces;
using System.Security.Claims;

namespace NotificationService.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/notifications")]
    public class NotificationController : ControllerBase
    {
        private readonly INotificationService _svc;
        public NotificationController(INotificationService svc) => _svc = svc;

        private int UserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        [HttpGet]
        public async Task<ActionResult<IEnumerable<NotificationResponseDto>>> Get([FromQuery] int limit = 50) =>
            Ok(await _svc.GetByUserAsync(UserId, limit));

        [HttpGet("unread")]
        public async Task<ActionResult<IEnumerable<NotificationResponseDto>>> Unread() =>
            Ok(await _svc.GetUnreadAsync(UserId));

        [HttpGet("unread-count")]
        public async Task<ActionResult<UnreadCountDto>> UnreadCount() =>
            Ok(new UnreadCountDto(await _svc.GetUnreadCountAsync(UserId)));

        [HttpPut("{id:int}/read")]
        public async Task<IActionResult> MarkRead(int id)
        {
            await _svc.MarkAsReadAsync(id, UserId);
            return NoContent();
        }

        [HttpPut("read-all")]
        public async Task<IActionResult> MarkAllRead()
        {
            await _svc.MarkAllReadAsync(UserId);
            return NoContent();
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            await _svc.DeleteAsync(id, UserId);
            return NoContent();
        }

        // ---------- Admin ----------
        [Authorize(Roles = "Admin")]
        [HttpPost("broadcast")]
        public async Task<ActionResult<IEnumerable<NotificationResponseDto>>> Broadcast(BroadcastNotificationDto dto)
        {
            // Admin must supply UserIds — resolving "all users" requires AuthService integration.
            var userIds = dto.UserIds ?? new List<int>();
            var result = await _svc.SendBulkAsync(dto, userIds);
            return Ok(result);
        }

        // Internal endpoint for other services (e.g. system) to push a notification to a specific user.
        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<ActionResult<NotificationResponseDto>> Create(CreateNotificationDto dto) =>
            Ok(await _svc.SendAsync(dto));
    }
}