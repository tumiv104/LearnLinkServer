using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs.User
{
    public class UserProfileDTO
    {
        public int UserId { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string RoleName { get; set; }
        public DateTime? Dob { get; set; }
        public string? AvatarUrl { get; set; }
        public DateTime CreatedAt { get; set; }

        public int TotalPoints { get; set; }

        // Nếu là Parent:
        public int ChildrenCount { get; set; }

        // Nếu là Child:
        public string? ParentName { get; set; }
    }
}
