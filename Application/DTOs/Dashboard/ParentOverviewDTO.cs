using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs.Dashboard
{
    public class ParentOverviewDTO
    {
        public OverviewStatsDTO Overview { get; set; } = new();
        public List<ChildSummaryDTO> Children { get; set; } = new();
        public List<NotificationDTO> RecentNotifications { get; set; } = new();
    }
}
