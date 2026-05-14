using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs.Dashboard
{
    public class OverviewStatsDTO
    {
        public int TotalMissions { get; set; }
        public int CompletedMissions { get; set; }
        public int PendingSubmissions { get; set; }
        public int PendingRedemptions { get; set; }
    }
}
