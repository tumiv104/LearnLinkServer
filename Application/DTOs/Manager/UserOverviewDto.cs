using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs.Manager
{
    public class UserOverviewDto
    {
        public int UserId { get; set; }
        public string Name { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string Role { get; set; } = null!;
        public int MissionsAssigned { get; set; }
        public int MissionsCompleted { get; set; }
        public int Points { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
