using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs.Payment
{
    public class CreatePaymentRequest
    {
        public int ParentId { get; set; }
        public long Amount { get; set; }
    }
}
