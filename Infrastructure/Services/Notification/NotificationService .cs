using Application.DTOs.Notification;
using Application.Interfaces.Notification;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Services.Notification
{
    public class NotificationService : INotificationService
    {
        private readonly LearnLinkDbContext _context;

        public NotificationService(LearnLinkDbContext context)
        {
            _context = context;
        }

        public async Task<List<NotificationResponseDTO>> GetAllByUserAsync(int userId)
        {
            return await _context.Notifications
                .Where(n => n.UserId == userId)
                .OrderByDescending(n => n.CreatedAt)
                .Select(n => new NotificationResponseDTO
                {
                    NotificationId = n.NotificationId,
                    UserId = n.UserId,
                    Type = n.Type.ToString(),
                    Payload = n.Payload,
                    IsRead = n.IsRead,
                    CreatedAt = n.CreatedAt
                })
                .ToListAsync();
        }

        public async Task<NotificationResponseDTO> AddNotificationAsync(NotificationRequestDTO dto)
        {
            var notification = new Domain.Entities.Notification
            {
                UserId = dto.UserId,
                Type = dto.Type,
                Payload = dto.Payload,
                CreatedAt = DateTime.UtcNow,
                IsRead = false
            };

            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();

            return new NotificationResponseDTO
            {
                NotificationId = notification.NotificationId,
                UserId = notification.UserId,
                Type = notification.Type.ToString(),
                Payload = notification.Payload,
                IsRead = notification.IsRead,
                CreatedAt = notification.CreatedAt
            };
        }

        public async Task<bool> MarkAsReadAsync(int notificationId)
        {
            var notification = await _context.Notifications.FindAsync(notificationId);
            if (notification == null) return false;

            notification.IsRead = true;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> MarkAllAsReadAsync(int userId)
        {
            var notifications = await _context.Notifications
                .Where(n => n.UserId == userId && !n.IsRead)
                .ToListAsync();

            if (!notifications.Any()) return false;

            foreach (var n in notifications)
                n.IsRead = true;

            await _context.SaveChangesAsync();
            return true;
        }
    }
}
