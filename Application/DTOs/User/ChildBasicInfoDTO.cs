using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs.User
{
    public class ChildBasicInfoDTO
    {
        public int ChildId { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public DateTime? Dob { get; set; }
        public string? AvatarUrl { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
