using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class Shop { 
        [Key] 
        public int ShopId { get; set; } 
        [Required, MaxLength(255)] 
        public string ShopName { get; set; } = null!; 
        public string? ContactInfo { get; set; } 
        public string? Website { get; set; } 
        public bool IsActive { get; set; } = true; 
        public ICollection<Product>? Products { get; set; } 
    }
}
