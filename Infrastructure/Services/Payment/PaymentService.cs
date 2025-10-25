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
using Net.payOS.Types;
using Net.payOS;

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
            var pointRate = Int32.Parse(_config["Payment:PointRate"]);

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

            var config = _config.GetSection("Payment:MoMo");
            var partnerCode = config["PartnerCode"];
            var accessKey = config["AccessKey"];
            var secretKey = config["SecretKey"];
            var endpoint = config["Endpoint"];
            var returnUrl = config["ReturnUrl"];
            var notifyUrl = config["NotifyUrl"];
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

                var pointRate = int.Parse(_config["Payment:PointRate"]);
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
            var secretKey = _config["Payment:MoMo:SecretKey"];
            var rawHash =
                $"accessKey={_config["Payment:MoMo:AccessKey"]}" +
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

        private long GenerateOrderCode(int paymentId)
        {
            long timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            string orderCodeStr = $"{timestamp}{paymentId:D4}";

            return long.Parse(orderCodeStr);
        }

        private int ExtractPaymentIdFromOrderCode(string orderCodeStr)
        {
            string paymentIdStr = orderCodeStr[^4..];

            return int.Parse(paymentIdStr);
        }

        public async Task<bool> UpdatePaymentStatus(string orderCode, string status)
        {
            var paymentId = ExtractPaymentIdFromOrderCode(orderCode);
            var payment = await _context.Payments.Include(p => p.Parent).FirstOrDefaultAsync(p => p.PaymentId == paymentId);
            if (payment == null) return false;

            if (status == "success") // thanh toán thành công
            {
                payment.Status = PaymentStatus.Success;

                if (payment.Purpose == PaymentPurpose.TopUpPoints)
                {
                    // cộng điểm cho parent
                    var pointRate = int.Parse(_config["Payment:PointRate"]);
                    var points = (int)(payment.Amount / pointRate);
                    var point = await _context.Points.FirstOrDefaultAsync(w => w.UserId == payment.ParentId);
                    if (point != null)
                    {
                        point.Balance += points;
                    }
                }
                else if (payment.Purpose == PaymentPurpose.UpgradePremium)
                {
                    // upgrade to premium
                    payment.Parent.IsPremium = true;
                    payment.Parent.UpdatedAt = DateTime.UtcNow;
                }
            }
            else
            {
                payment.Status = PaymentStatus.Failed;
            }

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<string> CreatePayOSPayment(int parentId, decimal amount)
        {
            var config = _config.GetSection("Payment:PayOS");
            var clientId = config["ClientId"];
            var apiKey = config["ApiKey"];
            var checksumKey = config["ChecksumKey"];
            var returnUrl = config["ReturnUrl"];
            var cancelUrl = config["CancelUrl"];

            var payOS = new PayOS(clientId, apiKey, checksumKey);

            //var orderCode = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            var items = new List<ItemData>
            {
                new ItemData("Top up points LearnLink", 1, (int)amount)
            };

            var payment = new Domain.Entities.Payment
            {
                ParentId = parentId,
                Amount = amount,
                Currency = "VND",
                Method = "PayOS",
                Status = PaymentStatus.Pending,
                Purpose = PaymentPurpose.TopUpPoints,
                CreatedAt = DateTime.UtcNow
            };

            _context.Payments.Add(payment);
            await _context.SaveChangesAsync();

            var orderCode = GenerateOrderCode(payment.PaymentId);

            var paymentData = new PaymentData(
                orderCode,
                (int)amount,
                "Top up points via PayOS",
                items,
                cancelUrl,
                returnUrl
            );

            var response = await payOS.createPaymentLink(paymentData);

            return response.checkoutUrl;
        }

        public async Task<bool> HandlePayOSCallback(PayOSWebhookDto webhookData)
        {
            var config = _config.GetSection("Payment:PayOS");
            var clientId = config["ClientId"];
            var apiKey = config["ApiKey"];
            var checksumKey = config["ChecksumKey"];

            var payOS = new PayOS(clientId, apiKey, checksumKey);

            try
            {
                var sdkWebhook = new WebhookType(
                     webhookData.Code,
                     webhookData.Desc,
                     webhookData.Success,
                     new WebhookData(
                        orderCode: webhookData.Data.OrderCode,
                        amount: (int)webhookData.Data.Amount,
                        description: webhookData.Data.Description ?? "",
                        accountNumber: null,
                        reference: null,
                        transactionDateTime: webhookData.Data.TransactionDateTime.ToString("O") ?? DateTime.UtcNow.ToString("O"),
                        currency: "VND",
                        paymentLinkId: null,
                        code: webhookData.Code,
                        desc: webhookData.Desc,
                        counterAccountBankId: null,
                        counterAccountBankName: null,
                        counterAccountName: null,
                        counterAccountNumber: null,
                        virtualAccountName: null,
                        virtualAccountNumber: null
                     ),
                     webhookData.Signature
                 );

                var verified = payOS.verifyPaymentWebhookData(sdkWebhook);
                if (verified == null) return false;

                var payment = await _context.Payments.FirstOrDefaultAsync(p => p.PaymentId == verified.orderCode);
                if (payment == null) return false;

                if (verified.code == "00" || verified.desc.Contains("success", StringComparison.OrdinalIgnoreCase))
                {
                    payment.Status = PaymentStatus.Success;

                    var rate = int.Parse(_config["Payment:PointRate"]);
                    var points = (int)(payment.Amount / rate);

                    var point = await _context.Points.FirstOrDefaultAsync(w => w.UserId == payment.ParentId);
                    if (point != null)
                        point.Balance += points;
                }
                else
                {
                    payment.Status = PaymentStatus.Failed;
                }

                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<string> UpgradeToPremiumAsync(int userId, decimal amount)
        {
            var payment = new Domain.Entities.Payment
            {
                ParentId = userId,
                Amount = amount,
                Currency = "VND",
                Method = "PayOS", 
                Status = PaymentStatus.Pending,
                Purpose = PaymentPurpose.UpgradePremium,
                CreatedAt = DateTime.UtcNow
            };

            _context.Payments.Add(payment);
            await _context.SaveChangesAsync();

            var config = _config.GetSection("Payment:PayOS");
            var clientId = config["ClientId"];
            var apiKey = config["ApiKey"];
            var checksumKey = config["ChecksumKey"];
            var returnUrl = config["ReturnUrl"];
            var cancelUrl = config["CancelUrl"];

            var payOS = new PayOS(clientId, apiKey, checksumKey);

            var items = new List<ItemData>
                {
                    new ItemData("LearnLink Premium", 1, (int)amount)
                };

            var orderCode = GenerateOrderCode(payment.PaymentId);

            var paymentData = new PaymentData(
                orderCode,
                (int)amount,
                "LearnLink Premium Upgrade",
                items,
                cancelUrl,
                returnUrl
            );

            var response = await payOS.createPaymentLink(paymentData);

            return response.checkoutUrl;
        }

        public async Task<bool> HandlePayOSCallbackForPremium(PayOSWebhookDto webhookData)
        {
            var config = _config.GetSection("Payment:PayOS");
            var clientId = config["ClientId"];
            var apiKey = config["ApiKey"];
            var checksumKey = config["ChecksumKey"];

            var payOS = new PayOS(clientId, apiKey, checksumKey);

            try
            {
                var sdkWebhook = new WebhookType(
                     webhookData.Code,
                     webhookData.Desc,
                     webhookData.Success,
                     new WebhookData(
                        orderCode: webhookData.Data.OrderCode,
                        amount: (int)webhookData.Data.Amount,
                        description: webhookData.Data.Description ?? "",
                        accountNumber: null,
                        reference: null,
                        transactionDateTime: webhookData.Data.TransactionDateTime.ToString("O"),
                        currency: "VND",
                        paymentLinkId: null,
                        code: webhookData.Code,
                        desc: webhookData.Desc,
                        counterAccountBankId: null,
                        counterAccountBankName: null,
                        counterAccountName: null,
                        counterAccountNumber: null,
                        virtualAccountName: null,
                        virtualAccountNumber: null
                     ),
                     webhookData.Signature
                 );

                var verified = payOS.verifyPaymentWebhookData(sdkWebhook);
                if (verified == null) return false;

                var payment = await _context.Payments
                    .Include(p => p.Parent)
                    .FirstOrDefaultAsync(p => p.PaymentId == verified.orderCode);
                if (payment == null) return false;

                if (verified.code == "00" || verified.desc.Contains("success", StringComparison.OrdinalIgnoreCase))
                {
                    payment.Status = PaymentStatus.Success;

                    payment.Parent.IsPremium = true;
                    payment.Parent.UpdatedAt = DateTime.UtcNow;
                }
                else
                {
                    payment.Status = PaymentStatus.Failed;
                }

                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }


    }
}
