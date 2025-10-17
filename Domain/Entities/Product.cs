using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class Product { 
        [Key] 
        public int ProductId { get; set; } 
        [ForeignKey("Shop")] 
        public int ShopId { get; set; }
        public Shop Shop { get; set; } = null!; 
        [Required, MaxLength(255)] 
        public string Name { get; set; } = null!; 
        public string? Description { get; set; } 
        public string? ImageUrl { get; set; } 
        public int PricePoints { get; set; } 
        public int Stock { get; set; } 
        public bool IsActive { get; set; } = true; 
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow; 
        public ICollection<Redemption>? Redemptions { get; set; } 
    }
}
