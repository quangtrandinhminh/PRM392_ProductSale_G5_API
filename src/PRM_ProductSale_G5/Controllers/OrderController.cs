using Microsoft.AspNetCore.Mvc;
using Services.ApiModels;
using Services.ApiModels.Order;
using Services.Constants;
using Services.Services;
using System.Threading.Tasks;

namespace PRM_ProductSale_G5.Controllers
{
    [ApiController]
    public class OrderController(IServiceProvider serviceProvider) : Controller
    {
        private readonly IOrderService _orderService = serviceProvider.GetRequiredService<IOrderService>();

        [HttpPost]
        [Route(WebApiEndpoint.Order.CreateOrder)]
        public async Task<IActionResult> CreateOrder([FromBody] OrderRequest request)
        {
            return Ok(BaseResponse.OkResponseDto(await _orderService.CreateOrderAsync(request, HttpContext)));
        }

        [HttpGet]
        [Route(WebApiEndpoint.Order.GetOrder)]
        public async Task<IActionResult> GetOrder([FromRoute] int id)
        {
            return Ok(BaseResponse.OkResponseDto(await _orderService.GetOrderByIdAsync(id)));
        }

        [HttpGet]
        [Route(WebApiEndpoint.Order.GetOrdersByUser)]
        public async Task<IActionResult> GetOrdersByUser([FromRoute] int userId)
        {
            return Ok(BaseResponse.OkResponseDto(await _orderService.GetOrdersByUserAsync(userId)));
        }

        [HttpPut]
        [Route(WebApiEndpoint.Order.UpdateOrderStatus)]
        public async Task<IActionResult> UpdateOrderStatus([FromRoute] int id, [FromQuery] string newStatus)
        {
            return Ok(BaseResponse.OkResponseDto(await _orderService.UpdateOrderStatusAsync(id, newStatus)));
        }

        [HttpDelete]
        [Route(WebApiEndpoint.Order.DeleteOrder)]
        public async Task<IActionResult> DeleteOrder([FromRoute] int id)
        {
            return Ok(BaseResponse.OkResponseDto(await _orderService.DeleteOrderAsync(id)));
        }
    }
}