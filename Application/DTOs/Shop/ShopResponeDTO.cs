using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs.Shop
{
    public class ShopResponeDTO
    {
        public int ShopId { get; set; }
        public string ShopName { get; set; } = null!;
        public string? ContactInfo { get; set; }
        public string? Website { get; set; }
        public bool IsActive { get; set; }
    }
}
