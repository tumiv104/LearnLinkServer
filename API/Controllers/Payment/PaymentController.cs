using Application.DTOs.Payment;
using Application.Interfaces.Payment;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Net.payOS.Types;
using System.Diagnostics;

namespace API.Controllers.Payment
{
    [Route("api/[controller]")]
    [ApiController]
    public class PaymentController : BaseController
    {
        private readonly IPaymentService _paymentService;

        public PaymentController(IPaymentService paymentService)
        {
            _paymentService = paymentService;
        }

        [HttpPost("momo-create")]
        public async Task<IActionResult> CreatePayment([FromBody] CreatePaymentRequest request)
        {
            var payUrl = await _paymentService.CreateMoMoPayment(request.ParentId, request.Amount);
            return OkResponse<string>(payUrl);
        }

        [HttpPost("momo-callback")]
        public async Task<IActionResult> MoMoCallback([FromBody] MoMoCallbackRequest callback)
        {
            Debug.WriteLine("call momo-callback");
            var result = await _paymentService.HandleMoMoCallback(callback);
            if (result) return ErrorResponse("Call back failed");
            Debug.WriteLine("momo-callback true");
            return OkResponse(result);
        }

        [HttpPost("update-status")]
        public async Task<IActionResult> UpdatePaymentStatus(UpdatePaymentRequest updatePaymentRequest)
        {
            var res = await _paymentService.UpdatePaymentStatus(updatePaymentRequest.paymentId, updatePaymentRequest.status);
            return OkResponse(res);
        }

        [HttpPost("payos-create")]
        public async Task<IActionResult> CreatePayOS([FromBody] CreatePaymentRequest request)
        {
            var payUrl = await _paymentService.CreatePayOSPayment(request.ParentId, request.Amount);
            return OkResponse<string>(payUrl);
        }

        [HttpPost("payos-callback")]
        public async Task<IActionResult> PayOSCallback([FromBody] PayOSWebhookDto webhook)
        {
            var result = await _paymentService.HandlePayOSCallback(webhook);
            if (!result)
                return ErrorResponse("Callback verify failed");

            return OkResponse(true);
        }

    }

    public class UpdatePaymentRequest
    {
        public int paymentId { get; set; }
        public string status { get; set; }
    }
}
