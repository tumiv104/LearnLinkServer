using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs.Dashboard
{
    public class NotificationDTO
    {
        public int NotificationId { get; set; }
        public string Message { get; set; } = "";
        public DateTime CreatedAt { get; set; }
        public string Type { get; set; } = "";    
        public bool IsRead { get; set; }
    }
}
