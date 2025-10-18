using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs.Manager
{
    public class ParentPerformanceDto
    {
        public int ParentId { get; set; }
        public string ParentName { get; set; } = null!;
        public int AssignedMissions { get; set; }
        public int CompletedMissions { get; set; }
        public double CompletionRate { get; set; }
    }
}
