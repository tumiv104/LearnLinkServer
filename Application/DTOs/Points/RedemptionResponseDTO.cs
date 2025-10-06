using Domain.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs.Points
{
    public class RedemptionResponseDTO
    {
        public int RedemptionId { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public string ProductImage { get; set; }
        public string ShopName { get; set; }
        public int PointsSpent { get; set; }
        public int ChildId { get; set; }
        public string ChildName { get; set; }
        public string Status { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
