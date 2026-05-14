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
        public Task<bool> UpdatePaymentStatus(string orderCode, string status);

        Task<string> CreatePayOSPayment(int parentId, decimal amount);
        Task<bool> HandlePayOSCallback(PayOSWebhookDto webhookData);
        Task<string> UpgradeToPremiumAsync(int userId, decimal amount);
        Task<bool> HandlePayOSCallbackForPremium(PayOSWebhookDto webhookData);
    }
}
