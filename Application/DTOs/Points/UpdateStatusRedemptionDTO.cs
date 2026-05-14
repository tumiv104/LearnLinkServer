using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs.Points
{
    public class UpdateStatusRedemptionDTO
    {
        public int RedemptionId { get; set; }
        public string NewStatus { get; set; }
    }
}
