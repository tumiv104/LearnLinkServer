using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.Auth
{
    public class UserLoginDTO
    {
        [Required]
        public required string Email { get; set; }
        [Required]
        public required string Password { get; set; }
    }
}
