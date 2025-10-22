using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs.Manager
{
    public class ChildPerformanceDto
    {
        public int ChildId { get; set; }
        public string ChildName { get; set; } = null!;
        public int AssignedMissions { get; set; }
        public int CompletedMissions { get; set; }
        public int ValidSubmissions { get; set; }
        public int RejectedSubmissions { get; set; }
        public double CompletionRate { get; set; }
    }
}
