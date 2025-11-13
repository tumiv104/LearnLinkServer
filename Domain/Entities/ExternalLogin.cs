using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class ExternalLogin
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey("User")]
        public int UserId { get; set; }
        public User User { get; set; }

        [Required, MaxLength(50)]
        public string Provider { get; set; } // "Google"

        [Required, MaxLength(200)]
        public string ProviderKey { get; set; } // Google user ID (sub in id_token)

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
