using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs.User
{
    public class UserProfileUpdateDTO
    {
        public string? Name { get; set; }
        public DateTime? Dob { get; set; }
        public string? AvatarUrl { get; set; }
        public IFormFile? AvatarFile { get; set; }
    }
}
