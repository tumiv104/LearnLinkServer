using Application.DTOs.Payment;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces.Payment
{
    public interface IPaymentService
    {
        public Task<string> CreateMoMoPayment(int userId, decimal amount);

        public Task<bool> HandleMoMoCallback(MoMoCallbackRequest callback);
        public Task<bool> UpdatePaymentStatus(int paymentId, string status);
    }
}
