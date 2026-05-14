using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities
{
    public class Point
    {
        [Key]
        public int PointsId { get; set; }

        [ForeignKey("User")]
        public int UserId { get; set; }  
        public User User { get; set; }  

        public int Balance { get; set; }

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }

}
