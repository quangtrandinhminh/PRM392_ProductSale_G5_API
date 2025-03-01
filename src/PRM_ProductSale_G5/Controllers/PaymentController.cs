using Microsoft.AspNetCore.Mvc;
using Service.ApiModels.VNPay;
using Services.ApiModels;
using Services.Constants;
using Services.Services;

namespace RentEZ.WebAPI.Controllers
{
    [Route("api/payments")]
    [ApiController]
    public class PaymentController(IServiceProvider serviceProvider) : ControllerBase
    {
        private readonly VnPayService _vnPayService = serviceProvider.GetRequiredService<VnPayService>();

        [HttpPost(WebApiEndpoint.Payment.VnpayUrl)]
        public async Task<IActionResult> GetVnPayPaymentUrl([FromBody] VnPaymentRequest request)
        {
            var response = await _vnPayService.CreatePaymentUrl(HttpContext, request);
            return Ok(BaseResponse.OkResponseDto(response));
        }

        [HttpPost(WebApiEndpoint.Payment.VnpayExecute)]
        public async Task<IActionResult> VnPayPaymentExecute(HttpContext context, int orderId)
        {
            var response = await _vnPayService.PaymentExecute(context, orderId);
            return Ok(BaseResponse.OkResponseDto(response));
        }
    }
}
