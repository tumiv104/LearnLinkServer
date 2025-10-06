using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs.Shop
{
    public class ShopRequestDTO
    {
        public string ShopName { get; set; } = null!;
        public string? ContactInfo { get; set; }
        public string? Website { get; set; }
        public bool IsActive { get; set; }
    }
}
