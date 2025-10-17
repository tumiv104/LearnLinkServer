using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs.Notification
{
    public class NotificationResponseDTO
    {
        public int NotificationId { get; set; }
        public int UserId { get; set; }
        public string Type { get; set; }
        public string Payload { get; set; } = "{}";
        public bool IsRead { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
