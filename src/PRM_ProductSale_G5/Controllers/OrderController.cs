using Microsoft.AspNetCore.Mvc;
using Services.ApiModels;
using Services.ApiModels.Order;
using Services.Constants;
using Services.Services;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using PRM_ProductSale_G5.Hubs;
using Services.ApiModels.Notification;
using Services.Helper;
using Service.Utils;
using System;

namespace PRM_ProductSale_G5.Controllers
{
    [ApiController]
    public class OrderController : Controller
    {
        private readonly IOrderService _orderService;
        private readonly INotificationService _notificationService;
        private readonly IHubContext<NotificationHub> _notificationHubContext;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IFirebaseService _firebaseService;

        public OrderController(IServiceProvider serviceProvider)
        {
            _orderService = serviceProvider.GetRequiredService<IOrderService>();
            _notificationService = serviceProvider.GetRequiredService<INotificationService>();
            _notificationHubContext = serviceProvider.GetRequiredService<IHubContext<NotificationHub>>();
            _httpContextAccessor = serviceProvider.GetRequiredService<IHttpContextAccessor>();
            _firebaseService = serviceProvider.GetRequiredService<IFirebaseService>();
        }

        [HttpPost]
        [Route(WebApiEndpoint.Order.CreateOrder)]
        public async Task<IActionResult> CreateOrder([FromBody] OrderRequest request)
        {
            var result = await _orderService.CreateOrderAsync(request, HttpContext);
            
            // Gửi thông báo cho người dùng về việc tạo đơn hàng mới
            var currentUser = JwtClaimUltils.GetLoginedUser(_httpContextAccessor);
            var userId = JwtClaimUltils.GetUserId(currentUser);
            
            await SendOrderNotification(
                result.OrderId, 
                userId, 
                "Đơn hàng mới của bạn đã được tạo và đang chờ xử lý"
            );
            
            return Ok(BaseResponse.OkResponseDto(result));
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
            var result = await _orderService.UpdateOrderStatusAsync(orderId, newStatus);
            
            // Lấy thông tin đơn hàng để xác định người dùng
            var order = await _orderService.GetOrderByIdAsync(orderId);
            
            if (order != null && order.UserId.HasValue)
            {
                await SendOrderNotification(
                    orderId,
                    order.UserId.Value,
                    $"Trạng thái đơn hàng của bạn đã được cập nhật thành {newStatus}"
                );
            }
            
            return Ok(BaseResponse.OkResponseDto(result));
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
            var result = await _orderService.CustomerChangeOrderStatusAsync(orderId);
            
            // Lấy thông tin đơn hàng sau khi cập nhật
            var order = await _orderService.GetOrderByIdAsync(orderId);
            
            if (order != null && order.UserId.HasValue)
            {
                await SendOrderNotification(
                    orderId,
                    order.UserId.Value,
                    $"Bạn đã xác nhận đã nhận được đơn hàng #{orderId}"
                );
            }
            
            return Ok(BaseResponse.OkResponseDto(result));
        }

        [HttpPut]
        [Authorize(Roles = "Admin")]
        [Route(WebApiEndpoint.Order.AdminChangeOrderStatus)]
        public async Task<IActionResult> AdminChangeOrderStatus([FromRoute] int orderId)
        {
            var result = await _orderService.AdminChangeOrderStatusAsync(orderId);
            
            // Lấy thông tin đơn hàng sau khi cập nhật
            var order = await _orderService.GetOrderByIdAsync(orderId);
            
            if (order != null && order.UserId.HasValue)
            {
                var status = order.OrderStatus;
                string message = $"Đơn hàng #{orderId} của bạn đã được cập nhật thành \"{status}\"";
                
                await SendOrderNotification(orderId, order.UserId.Value, message);
            }
            
            return Ok(BaseResponse.OkResponseDto(result));
        }

        [HttpDelete]
        [Authorize(Roles = "Customer")]
        [Route(WebApiEndpoint.Order.CustomerCancelOrder)]
        public async Task<IActionResult> CustomerCancelOrder([FromRoute] int orderId)
        {
            var result = await _orderService.CustomerCancelOrderAsync(orderId);
            
            // Lấy thông tin người dùng hiện tại
            var currentUser = JwtClaimUltils.GetLoginedUser(_httpContextAccessor);
            var userId = JwtClaimUltils.GetUserId(currentUser);
            
            await SendOrderNotification(
                orderId,
                userId,
                $"Bạn đã hủy đơn hàng #{orderId}"
            );
            
            return Ok(BaseResponse.OkResponseDto(result));
        }

        [HttpGet]
        [Route(WebApiEndpoint.Order.GetOrdersByStatus)]
        public async Task<IActionResult> GetOrdersByStatus([FromQuery] string? status)
        {
            return Ok(BaseResponse.OkResponseDto(await _orderService.GetOrdersByStatusAsync(status)));
        }

        
        /// <summary>
        /// Gửi thông báo cho người dùng khi có thay đổi trong đơn hàng
        /// </summary>
        private async Task SendOrderNotification(int orderId, int userId, string message)
        {
            try
            {
                // Tạo thông báo trong hệ thống
                var notificationRequest = new NotificationCreateRequest
                {
                    UserId = userId,
                    Message = message
                };
                
                var notification = await _notificationService.CreateNotificationAsync(notificationRequest);
                
                // Gửi thông báo qua SignalR
                if (NotificationHub._userConnectionMap.TryGetValue(userId, out var connectionId))
                {
                    await _notificationHubContext.Clients.Client(connectionId)
                        .SendAsync("ReceiveNotification", notification);
                }
                
                // Tạo dữ liệu thông báo đẩy
                var notificationData = PushNotificationHelper.CreateOrderNotificationData(orderId, message);
                
                // Gửi thông báo đẩy Firebase đến người dùng
                try
                {
                    // Gửi đến chủ đề "user-{userId}" - giả định rằng người dùng đã đăng ký ở client
                    await _firebaseService.SendNotificationToTopicAsync(
                        $"user-{userId}",
                        "Cập nhật đơn hàng",
                        message,
                        notificationData
                    );
                }
                catch (Exception ex)
                {
                    // Log lỗi nhưng không dừng quá trình xử lý
                    Console.WriteLine($"Lỗi khi gửi thông báo đẩy: {ex.Message}");
                }
            }
            catch (Exception ex)
            {
                // Log lỗi nhưng không dừng quá trình xử lý
                Console.WriteLine($"Lỗi khi gửi thông báo: {ex.Message}");
            }
        }
    }
}