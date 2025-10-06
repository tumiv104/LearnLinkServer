using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs.Product
{
    public class ProductRequestDTO
    {
        public int ShopId { get; set; }
        public string Name { get; set; } = null!; 
        public string? Description { get; set; }
        public string? ImageUrl { get; set; }
        public int PricePoints { get; set; }
        public int Stock { get; set; }
        public bool IsActive { get; set; }
    }
}
