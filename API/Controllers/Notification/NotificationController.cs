using Application.DTOs.Notification;
using Application.Interfaces.Notification;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers.Notification
{
    [Route("api/[controller]")]
    [ApiController]
    public class NotificationController : BaseController
    {
        private readonly INotificationService _notificationService;

        public NotificationController(INotificationService notificationService)
        {
            _notificationService = notificationService;
        }

        [HttpGet("getAll/{userId}")]
        public async Task<IActionResult> GetAllByUser(int userId)
        {
            var result = await _notificationService.GetAllByUserAsync(userId);
            return OkResponse(result);
        }

        [HttpPost("add")]
        public async Task<IActionResult> AddNotification([FromBody] NotificationRequestDTO dto)
        {
            var notification = await _notificationService.AddNotificationAsync(dto);
            return OkResponse(notification);
        }

        [HttpPut("markRead/{id}")]
        public async Task<IActionResult> MarkAsRead(int id)
        {
            var success = await _notificationService.MarkAsReadAsync(id);
            if (!success) return NotFoundResponse();
            return OkResponse(success);
        }

        [HttpPut("markAllRead/{userId}")]
        public async Task<IActionResult> MarkAllAsRead(int userId)
        {
            var success = await _notificationService.MarkAllAsReadAsync(userId);
            if (!success) return NotFoundResponse();
            return OkResponse(success);
        }
    }
}
