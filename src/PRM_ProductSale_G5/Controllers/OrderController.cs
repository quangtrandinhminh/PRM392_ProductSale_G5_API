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
using System.Collections.Generic;
using Serilog;
using ILogger = Serilog.ILogger;

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
        private readonly IUserDeviceService _userDeviceService;
        private readonly ILogger _logger;

        public OrderController(IServiceProvider serviceProvider)
        {
            _orderService = serviceProvider.GetRequiredService<IOrderService>();
            _notificationService = serviceProvider.GetRequiredService<INotificationService>();
            _notificationHubContext = serviceProvider.GetRequiredService<IHubContext<NotificationHub>>();
            _httpContextAccessor = serviceProvider.GetRequiredService<IHttpContextAccessor>();
            _firebaseService = serviceProvider.GetRequiredService<IFirebaseService>();
            _userDeviceService = serviceProvider.GetRequiredService<IUserDeviceService>();
            _logger = serviceProvider.GetRequiredService<ILogger>();
        }

        [HttpPost]
        [Route(WebApiEndpoint.Order.CreateOrder)]
        public async Task<IActionResult> CreateOrder([FromBody] OrderRequest request)
        {
            _logger.Information("Đang tạo đơn hàng mới");
            var result = await _orderService.CreateOrderAsync(request, HttpContext);
            _logger.Information("Đã tạo đơn hàng mới: OrderId={OrderId}", result.OrderId);
            
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
            _logger.Information("Đang cập nhật trạng thái đơn hàng {OrderId} thành {NewStatus}", orderId, newStatus);
            var result = await _orderService.UpdateOrderStatusAsync(orderId, newStatus);
            _logger.Information("Đã cập nhật trạng thái đơn hàng {OrderId} thành công", orderId);
            
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
            _logger.Information("Khách hàng đang thay đổi trạng thái đơn hàng {OrderId}", orderId);
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
                
                _logger.Information("Khách hàng đã xác nhận nhận đơn hàng {OrderId}, trạng thái mới: {Status}", 
                                 orderId, order.OrderStatus);
            }
            
            return Ok(BaseResponse.OkResponseDto(result));
        }

        [HttpPut]
        [Authorize(Roles = "Admin")]
        [Route(WebApiEndpoint.Order.AdminChangeOrderStatus)]
        public async Task<IActionResult> AdminChangeOrderStatus([FromRoute] int orderId)
        {
            _logger.Information("Admin đang thay đổi trạng thái đơn hàng {OrderId}", orderId);
            var result = await _orderService.AdminChangeOrderStatusAsync(orderId);
            
            // Lấy thông tin đơn hàng sau khi cập nhật
            var order = await _orderService.GetOrderByIdAsync(orderId);
            
            if (order != null && order.UserId.HasValue)
            {
                var status = order.OrderStatus;
                string message = $"Đơn hàng #{orderId} của bạn đã được cập nhật thành \"{status}\"";
                
                await SendOrderNotification(orderId, order.UserId.Value, message);
                
                _logger.Information("Admin đã thay đổi trạng thái đơn hàng {OrderId} thành {Status}", 
                                 orderId, status);
            }
            
            return Ok(BaseResponse.OkResponseDto(result));
        }

        [HttpDelete]
        [Authorize(Roles = "Customer")]
        [Route(WebApiEndpoint.Order.CustomerCancelOrder)]
        public async Task<IActionResult> CustomerCancelOrder([FromRoute] int orderId)
        {
            _logger.Information("Khách hàng đang hủy đơn hàng {OrderId}", orderId);
            var result = await _orderService.CustomerCancelOrderAsync(orderId);
            
            // Lấy thông tin người dùng hiện tại
            var currentUser = JwtClaimUltils.GetLoginedUser(_httpContextAccessor);
            var userId = JwtClaimUltils.GetUserId(currentUser);
            
            await SendOrderNotification(
                orderId,
                userId,
                $"Bạn đã hủy đơn hàng #{orderId}"
            );
            
            _logger.Information("Khách hàng đã hủy đơn hàng {OrderId} thành công", orderId);
            
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
                _logger.Information("Đang gửi thông báo về đơn hàng {OrderId} đến người dùng {UserId}: {Message}", 
                                 orderId, userId, message);
                
                // Tạo thông báo trong hệ thống
                var notificationRequest = new NotificationCreateRequest
                {
                    UserId = userId,
                    Message = message
                };
                
                var notification = await _notificationService.CreateNotificationAsync(notificationRequest);
                _logger.Information("Đã tạo thông báo NotificationId={NotificationId}", notification.NotificationId);
                
                // Gửi thông báo qua SignalR
                await SendNotificationViaSignalR(notification);
                
                // Gửi thông báo qua Firebase
                await SendPushNotificationViaFirebase(notification, orderId);
            }
            catch (Exception ex)
            {
                // Log lỗi nhưng không dừng quá trình xử lý
                _logger.Error(ex, "Lỗi khi gửi thông báo về đơn hàng {OrderId}: {ErrorMessage}", 
                           orderId, ex.Message);
            }
        }
        
        /// <summary>
        /// Gửi thông báo qua SignalR
        /// </summary>
        private async Task SendNotificationViaSignalR(NotificationDto notification)
        {
            try
            {
                if (notification.UserId.HasValue)
                {
                    _logger.Information("Đang gửi thông báo SignalR cho người dùng {UserId}, NotificationId: {NotificationId}", 
                                    notification.UserId.Value, notification.NotificationId);
                    
                    // Gửi thông báo qua SignalR sử dụng connectionId từ mapping
                    if (NotificationHub._userConnectionMap.TryGetValue(notification.UserId.Value, out var connectionId))
                    {
                        _logger.Information("Người dùng {UserId} đang kết nối với SignalR, gửi thông báo qua connectionId {ConnectionId}", 
                                        notification.UserId.Value, connectionId);
                        
                        await _notificationHubContext.Clients.Client(connectionId).SendAsync("ReceiveNotification", notification);
                        
                        _logger.Information("Đã gửi thông báo SignalR đến connectionId {ConnectionId} thành công", connectionId);
                    }
                    else
                    {
                        _logger.Warning("Người dùng {UserId} không kết nối với NotificationHub, không thể gửi qua connectionId", 
                                    notification.UserId.Value);
                    }
                    
                    // Gửi thông báo đến nhóm người dùng
                    var groupName = $"user_{notification.UserId.Value}_notifications";
                    _logger.Information("Đang gửi thông báo SignalR đến nhóm {GroupName}", groupName);
                    
                    await _notificationHubContext.Clients.Group(groupName).SendAsync("ReceiveNotification", notification);
                    
                    _logger.Information("Đã gửi thông báo SignalR đến nhóm {GroupName} thành công", groupName);
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Lỗi khi gửi thông báo qua SignalR: {ErrorMessage}", ex.Message);
            }
        }
        
        /// <summary>
        /// Gửi push notification qua Firebase
        /// </summary>
        private async Task SendPushNotificationViaFirebase(NotificationDto notification, int orderId)
        {
            try
            {
                if (!notification.UserId.HasValue)
                {
                    _logger.Warning("Không thể gửi push notification vì UserId là null");
                    return;
                }
                
                // Lấy danh sách thiết bị của người dùng
                var userDevices = await _userDeviceService.GetUserDevicesAsync(notification.UserId.Value);
                var deviceList = userDevices.ToList();
                
                if (deviceList.Count == 0)
                {
                    _logger.Warning("Người dùng {UserId} không có thiết bị nào được đăng ký", notification.UserId.Value);
                    return;
                }
                
                _logger.Information("Tìm thấy {DeviceCount} thiết bị của người dùng {UserId}", 
                                deviceList.Count, notification.UserId.Value);
                
                // Chuẩn bị dữ liệu thông báo
                var data = PushNotificationHelper.CreateOrderNotificationData(orderId, notification.Message);
                _logger.Information("Đã tạo dữ liệu thông báo đơn hàng cho OrderId={OrderId}", orderId);
                
                // Gửi thông báo đến từng thiết bị của người dùng
                foreach (var device in deviceList)
                {
                    string tokenPreview = device.DeviceToken.Length > 10 
                        ? device.DeviceToken.Substring(0, 10) + "..." 
                        : device.DeviceToken;
                        
                    _logger.Information("Đang gửi push notification đến thiết bị {DeviceToken}", tokenPreview);
                    
                    var result = await _firebaseService.SendNotificationAsync(
                        device.DeviceToken,
                        "Cập nhật đơn hàng",
                        notification.Message,
                        data
                    );
                    
                    if (result)
                    {
                        _logger.Information("Đã gửi push notification thành công đến thiết bị {DeviceId}", device.UserDeviceId);
                    }
                    else
                    {
                        _logger.Warning("Không thể gửi push notification đến thiết bị {DeviceId}", device.UserDeviceId);
                    }
                }
                
                // Gửi thông báo đến chủ đề riêng của người dùng
                var userTopic = $"user-{notification.UserId.Value}";
                _logger.Information("Đang gửi push notification đến chủ đề {Topic}", userTopic);
                
                var topicResult = await _firebaseService.SendNotificationToTopicAsync(
                    userTopic,
                    "Cập nhật đơn hàng",
                    notification.Message,
                    data
                );
                
                if (topicResult)
                {
                    _logger.Information("Đã gửi push notification thành công đến chủ đề {Topic}", userTopic);
                }
                else
                {
                    _logger.Warning("Không thể gửi push notification đến chủ đề {Topic}", userTopic);
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Lỗi khi gửi push notification qua Firebase: {ErrorMessage}", ex.Message);
            }
        }
    }
}