using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs.Notification
{
    public class NotificationRequestDTO
    {
        public int UserId { get; set; }
        public NotificationType Type { get; set; }
        public string Payload { get; set; } = "{}";
    }
}
