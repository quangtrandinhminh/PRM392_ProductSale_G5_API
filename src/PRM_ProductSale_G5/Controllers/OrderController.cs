using Microsoft.AspNetCore.Mvc;
using Services.ApiModels;
using Services.ApiModels.Order;
using Services.Constants;
using Services.Services;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;

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
        public async Task<IActionResult> GetOrder([FromRoute] int orderId)
        {
            return Ok(BaseResponse.OkResponseDto(await _orderService.GetOrderByIdAsync(orderId)));
        }

        [HttpGet]
        [Route(WebApiEndpoint.Order.GetOrdersByUser)]
        public async Task<IActionResult> GetOrdersByUser([FromQuery] string status)
        {
            var orders = await _orderService.GetOrdersByUserAsync(status);
            return Ok(BaseResponse.OkResponseDto(orders));
        }

        [HttpPut]
        [Route(WebApiEndpoint.Order.UpdateOrderStatus)]
        public async Task<IActionResult> UpdateOrderStatus([FromRoute] int orderId, [FromQuery] string newStatus)
        {
            return Ok(BaseResponse.OkResponseDto(await _orderService.UpdateOrderStatusAsync(orderId, newStatus)));
        }

        [HttpDelete]
        [Route(WebApiEndpoint.Order.DeleteOrder)]
        public async Task<IActionResult> DeleteOrder([FromRoute] int orderId)
        {
            return Ok(BaseResponse.OkResponseDto(await _orderService.DeleteOrderAsync(orderId)));
        }

        [HttpPut]
        [Authorize(Roles = "Customer")]
        [Route(WebApiEndpoint.Order.CustomerChangeOrderStatus)]
        public async Task<IActionResult> CustomerChangeOrderStatus([FromRoute] int orderId)
        {
            return Ok(BaseResponse.OkResponseDto(await _orderService.CustomerChangeOrderStatusAsync(orderId)));
        }

        [HttpPut]
        [Authorize(Roles = "Admin")]
        [Route(WebApiEndpoint.Order.AdminChangeOrderStatus)]
        public async Task<IActionResult> AdminChangeOrderStatus([FromRoute] int orderId)
        {
            return Ok(BaseResponse.OkResponseDto(await _orderService.AdminChangeOrderStatusAsync(orderId)));
        }

        [HttpDelete]
        [Authorize(Roles = "Customer")]
        [Route(WebApiEndpoint.Order.CustomerCancelOrder)]
        public async Task<IActionResult> CustomerCancelOrder([FromRoute] int orderId)
        {
            return Ok(BaseResponse.OkResponseDto(await _orderService.CustomerCancelOrderAsync(orderId)));
        }

        [HttpGet]
        [Route(WebApiEndpoint.Order.GetOrdersByStatus)]
        public async Task<IActionResult> GetOrdersByStatus([FromQuery] string? status)
        {
            return Ok(BaseResponse.OkResponseDto(await _orderService.GetOrdersByStatusAsync(status)));
        }

       }
}