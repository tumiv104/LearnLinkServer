using Application.DTOs.Notification;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces.Notification
{
    public interface INotificationService
    {
        Task<List<NotificationResponseDTO>> GetAllByUserAsync(int userId);
        Task<NotificationResponseDTO> AddNotificationAsync(NotificationRequestDTO dto);
        Task<bool> MarkAsReadAsync(int notificationId);
        Task<bool> MarkAllAsReadAsync(int userId);
    }
}
