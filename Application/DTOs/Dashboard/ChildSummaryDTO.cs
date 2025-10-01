using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs.Dashboard
{
    public class ChildSummaryDTO
    {
        public int UserId { get; set; }           
        public string Name { get; set; } = "";  
        public string AvatarUrl { get; set; } = "";
        public int TotalPoints { get; set; }
    }
}
