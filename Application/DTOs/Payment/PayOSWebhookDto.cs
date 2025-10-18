using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs.Payment
{
    public class PayOSWebhookDto
    {
        public string Code { get; set; }
        public string Desc { get; set; }
        public bool Success { get; set; }
        public WebhookDataDto Data { get; set; }
        public string Signature { get; set; }
    }

    public class WebhookDataDto
    {
        public long OrderCode { get; set; }
        public long Amount { get; set; }
        public string Description { get; set; }
        public DateTime TransactionDateTime { get; set; }
        public string TransactionId { get; set; }
    }
}
