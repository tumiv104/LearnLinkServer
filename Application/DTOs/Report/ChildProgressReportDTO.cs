using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs.Report
{
    public class ChildProgressReportDTO
    {
        public int ChildId { get; set; }
        public string ChildName { get; set; }
        public int TotalMissions { get; set; }
        public int CompletedMissions { get; set; }
        public int SubmittedMissions { get; set; }
        public int ProcessingMissions { get; set; }
        public int AssignedMissions { get; set; }
        public int TotalPointsEarned { get; set; }
        public DateTime? LastSubmissionAt { get; set; }
    }
}
