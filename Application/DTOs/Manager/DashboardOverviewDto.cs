using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs.Manager
{
    public class DashboardOverviewDto
    {
        public int TotalParents { get; set; }
        public int TotalChildren { get; set; }
        public int TotalMissions { get; set; }
        public int PendingMissions { get; set; }
        public int CompletedMissions { get; set; }
        public int TotalSubmissions { get; set; }
        public int PendingSubmissions { get; set; }
        public int ApprovedSubmissions { get; set; }
        public int RejectedSubmissions { get; set; }
        public double CompletionRate { get; set; }
        public int TodayTasksAssigned { get; set; }
        public int TodayTasksSubmitted { get; set; }
    }
}
