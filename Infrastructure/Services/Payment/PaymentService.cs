using Application.DTOs.Payment;
using Application.Interfaces.Payment;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Infrastructure.Data;
using System.Diagnostics;

namespace Infrastructure.Services.Payment
{
    public class PaymentService : IPaymentService
    {
        private readonly IConfiguration _config;
        private readonly HttpClient _http;
        private readonly LearnLinkDbContext _context;

        public PaymentService(IConfiguration config, HttpClient http, LearnLinkDbContext context)
        {
            _config = config;
            _http = http;
            _context = context;
        }

        public async Task<string> CreateMoMoPayment(int userId, decimal amount)
        {
            var pointRate = Int32.Parse(_config["MoMo:PointRate"]);

            var points = (int)(amount/pointRate); // 1000 VND = 1 points
            var payment = new Domain.Entities.Payment
            {
                ParentId = userId,
                Amount = amount,
                Currency = "VND",
                Method = "MoMo",
                Status = PaymentStatus.Pending,
                CreatedAt = DateTime.UtcNow
            };
            _context.Payments.Add(payment);
            await _context.SaveChangesAsync();


            var partnerCode = _config["MoMo:PartnerCode"];
            var accessKey = _config["MoMo:AccessKey"];
            var secretKey = _config["MoMo:SecretKey"];
            var endpoint = _config["MoMo:Endpoint"];
            var returnUrl = _config["MoMo:ReturnUrl"];
            var notifyUrl = _config["MoMo:NotifyUrl"];
            var requestId = Guid.NewGuid().ToString();
            var orderId = payment.PaymentId; 
            var amountStr = Convert.ToInt64(Math.Round(payment.Amount)).ToString();
            var extraData = payment.ParentId.ToString();
            var orderInfo = "Nạp điểm LearnLink";
            var requestType = "captureWallet";

            var rawHash =
                $"accessKey={accessKey}&amount={amountStr}&extraData={extraData}" +
                $"&ipnUrl={notifyUrl}&orderId={orderId}&orderInfo={orderInfo}" +
                $"&partnerCode={partnerCode}&redirectUrl={returnUrl}" +
                $"&requestId={requestId}&requestType={requestType}";

            var signature = SignSHA256(rawHash, secretKey);

            var body = new
            {
                partnerCode,
                partnerName = "LearnLink",
                storeId = "LearnLinkStore",
                requestId,
                amount,
                orderId,
                orderInfo,
                redirectUrl = returnUrl,
                ipnUrl = notifyUrl,
                lang = "vi",
                requestType,
                extraData,
                signature
            };

            var json = JsonSerializer.Serialize(body);
            var res = await _http.PostAsync(endpoint, new StringContent(json, Encoding.UTF8, "application/json"));
            var content = await res.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(content);
            return doc.RootElement.GetProperty("payUrl").GetString();
        }

        public async Task<bool> HandleMoMoCallback(MoMoCallbackRequest callback)
        {
            if (!VerifySignature(callback)) return false;

            var payment = await _context.Payments.FirstOrDefaultAsync(p => p.PaymentId.ToString() == callback.OrderId);
            if (payment == null) return false;

            if (callback.ResultCode == 0)
            {
                payment.Status = PaymentStatus.Success;

                var pointRate = int.Parse(_config["MoMo:PointRate"]);
                var points = (int)(callback.Amount * pointRate);
                var point = await _context.Points.FirstOrDefaultAsync(w => w.UserId == payment.ParentId);
                if (point != null)
                {
                    point.Balance += points;
                }
            }
            else
            {
                payment.Status = PaymentStatus.Failed;
            }

            await _context.SaveChangesAsync();
            return true;
        }

        public bool VerifySignature(MoMoCallbackRequest callback)
        {
            var secretKey = _config["MoMo:SecretKey"];
            var rawHash =
                $"accessKey={_config["MoMo:AccessKey"]}" +
                $"&amount={callback.Amount}" +
                $"&extraData={callback.ExtraData}" +
                $"&message={callback.Message}" +
                $"&orderId={callback.OrderId}" +
                $"&orderInfo={callback.OrderInfo}" +
                $"&orderType={callback.OrderType}" +
                $"&partnerCode={callback.PartnerCode}" +
                $"&payType={callback.PayType}" +
                $"&requestId={callback.RequestId}" +
                $"&responseTime={callback.ResponseTime}" +
                $"&resultCode={callback.ResultCode}" +
                $"&transId={callback.TransId}";

            var expected = SignSHA256(rawHash, secretKey);
            return expected == callback.Signature;
        }

        private static string SignSHA256(string data, string key)
        {
            using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(key));
            var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(data));
            return BitConverter.ToString(hash).Replace("-", "").ToLower();
        }

        public async Task<bool> UpdatePaymentStatus(int paymentId, string status)
        {
            Debug.WriteLine("paymentId: " + paymentId,"status: " + status);
            var payment = await _context.Payments.FirstOrDefaultAsync(p => p.PaymentId == paymentId);
            if (payment == null) return false;

            if (status == "success") // thanh toán thành công
            {
                payment.Status = PaymentStatus.Success;

                // cộng điểm cho parent
                var pointRate = int.Parse(_config["MoMo:PointRate"]);
                var points = (int)(payment.Amount/pointRate);
                var point = await _context.Points.FirstOrDefaultAsync(w => w.UserId == payment.ParentId);
                if (point != null)
                {
                    point.Balance += points;
                }
            }
            else
            {
                payment.Status = PaymentStatus.Failed;
            }

            await _context.SaveChangesAsync();
            return true;
        }
    }
}
