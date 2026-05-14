using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs.Manager
{
    public class ActivityChartDto
    {
        public string Day { get; set; } = string.Empty;
        public int Assigned { get; set; }
        public int Submitted { get; set; }
    }
}
