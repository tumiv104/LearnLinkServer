using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Domain.Entities
{
    public class RefreshToken
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        [Required]
        public string Token { get; set; } = null!;
        [Required]
        public DateTime Expires { get; set; }
        public bool IsRevoked { get; set; } = false;
        public DateTime? RevokedAt { get; set; }
        public int UserId { get; set; }
        public User User { get; set; } = null!;
        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
