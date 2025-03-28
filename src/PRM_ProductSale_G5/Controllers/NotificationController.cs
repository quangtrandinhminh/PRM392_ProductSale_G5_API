using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using PRM_ProductSale_G5.Hubs;
using Service.Utils;
using Services.ApiModels;
using Services.ApiModels.Notification;
using Services.ApiModels.PaginatedList;
using Services.Constants;
using Services.Services;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using ILogger = Serilog.ILogger;

namespace PRM_ProductSale_G5.Controllers
{
    [ApiController]
    [Authorize]
    public class NotificationController : ControllerBase
    {
        private readonly INotificationService _notificationService;
        private readonly IHubContext<NotificationHub> _notificationHubContext;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IFirebaseService _firebaseService;
        private readonly IUserDeviceService _userDeviceService;
        private readonly ILogger _logger;
        
        public NotificationController(IServiceProvider serviceProvider)
        {
            _notificationService = serviceProvider.GetRequiredService<INotificationService>();
            _notificationHubContext = serviceProvider.GetRequiredService<IHubContext<NotificationHub>>();
            _httpContextAccessor = serviceProvider.GetRequiredService<IHttpContextAccessor>();
            _firebaseService = serviceProvider.GetRequiredService<IFirebaseService>();
            _userDeviceService = serviceProvider.GetRequiredService<IUserDeviceService>();
            _logger = serviceProvider.GetRequiredService<ILogger>();
        }
        
        [HttpGet]
        [Route(WebApiEndpoint.Notification.GetNotifications)]
        public async Task<IActionResult> GetNotifications([FromQuery] PaginatedListRequest request)
        {
            var currentUser = JwtClaimUltils.GetLoginedUser(_httpContextAccessor);
            var currentUserId = JwtClaimUltils.GetUserId(currentUser);
            
            var notifications = await _notificationService.GetNotificationsByUserIdAsync(
                currentUserId, request.PageNumber, request.PageSize);
                
            return Ok(BaseResponse.OkResponseDto(notifications));
        }
        
        [HttpGet]
        [Route(WebApiEndpoint.Notification.GetNotification)]
        public async Task<IActionResult> GetNotification([FromRoute] int id)
        {
            var notification = await _notificationService.GetNotificationByIdAsync(id);
            return Ok(BaseResponse.OkResponseDto(notification));
        }
        
        [HttpPost]
        [Route(WebApiEndpoint.Notification.CreateNotification)]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateNotification([FromBody] NotificationCreateRequest request)
        {
            _logger.Information("Đang tạo thông báo mới cho người dùng {UserId}", request.UserId);
            var notification = await _notificationService.CreateNotificationAsync(request);
            
            // Thông báo thành công tạo thông báo
            _logger.Information("Đã tạo thông báo mới: NotificationId={NotificationId}, UserId={UserId}, Message={Message}", 
                            notification.NotificationId, notification.UserId, notification.Message);
            
            await SendNotificationViaSignalR(notification);
            await SendPushNotificationViaFirebase(notification);
            
            return Ok(BaseResponse.OkResponseDto(notification));
        }
        
        [HttpGet]
        [Route(WebApiEndpoint.Notification.GetBroadcastNotifications)]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetBroadcastNotifications([FromQuery] PaginatedListRequest request)
        {
            var broadcastNotifications = await _notificationService.GetBroadcastNotificationsAsync(
                request.PageNumber, request.PageSize);
                
            return Ok(BaseResponse.OkResponseDto(broadcastNotifications));
        }
        
        [HttpPost]
        [Route(WebApiEndpoint.Notification.SendNotificationToAllUsers)]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> SendNotificationToAllUsers([FromBody] SendNotificationToAllRequest request)
        {
            _logger.Information("Đang gửi thông báo quảng bá đến tất cả người dùng. Title: {Title}, Message: {Message}", 
                            request.Title, request.Message);
            
            var result = await _notificationService.SendNotificationToAllUsersAsync(request.Message, request.Title);
            
            // Thông báo qua SignalR cho tất cả người dùng
            _logger.Information("Đang gửi thông báo quảng bá qua SignalR");
            
            await _notificationHubContext.Clients.All.SendAsync("ReceiveBroadcastNotification", new 
            { 
                Title = request.Title, 
                Message = request.Message, 
                Timestamp = DateTime.UtcNow.AddHours(7) 
            });
            
            _logger.Information("Đã gửi thông báo quảng bá qua SignalR thành công");
            
            // Gửi Firebase Push Notification đến tất cả người dùng (theo chủ đề)
            try
            {
                // Chuẩn bị dữ liệu
                var data = new Dictionary<string, string>
                {
                    { "type", "broadcast" },
                    { "timestamp", DateTime.UtcNow.AddHours(7).ToString("o") }
                };
                
                _logger.Information("Đang gửi thông báo quảng bá qua Firebase đến chủ đề 'all-users'");
                
                // Gửi thông báo đến chủ đề "all-users"
                var firebaseResult = await _firebaseService.SendNotificationToTopicAsync(
                    "all-users",
                    request.Title,
                    request.Message,
                    data
                );
                
                if (firebaseResult)
                {
                    _logger.Information("Đã gửi thông báo quảng bá qua Firebase thành công");
                }
                else
                {
                    _logger.Warning("Gửi thông báo quảng bá qua Firebase thất bại");
                }
            }
            catch (Exception ex)
            {
                // Log lỗi nhưng không ngăn việc trả về kết quả
                _logger.Error(ex, "Lỗi khi gửi Firebase broadcast notification: {ErrorMessage}", ex.Message);
            }
            
            return Ok(BaseResponse.OkResponseDto(result));
        }
        
        [HttpPut]
        [Route(WebApiEndpoint.Notification.UpdateNotification)]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateNotification([FromBody] NotificationUpdateRequest request)
        {
            var result = await _notificationService.UpdateNotificationAsync(request);
            return Ok(BaseResponse.OkResponseDto(result));
        }
        
        [HttpDelete]
        [Route(WebApiEndpoint.Notification.DeleteNotification)]
        public async Task<IActionResult> DeleteNotification([FromRoute] int id)
        {
            var result = await _notificationService.DeleteNotificationAsync(id);
            return Ok(BaseResponse.OkResponseDto(result));
        }
        
        [HttpGet]
        [Route(WebApiEndpoint.Notification.GetUnreadCount)]
        public async Task<IActionResult> GetUnreadCount()
        {
            var currentUser = JwtClaimUltils.GetLoginedUser(_httpContextAccessor);
            var currentUserId = JwtClaimUltils.GetUserId(currentUser);
            
            var count = await _notificationService.GetUnreadCountAsync(currentUserId);
            return Ok(BaseResponse.OkResponseDto(count));
        }
        
        [HttpPut]
        [Route(WebApiEndpoint.Notification.MarkAsRead)]
        public async Task<IActionResult> MarkAsRead([FromRoute] int id)
        {
            var result = await _notificationService.MarkAsReadAsync(id);
            return Ok(BaseResponse.OkResponseDto(result));
        }
        
        [HttpPut]
        [Route(WebApiEndpoint.Notification.MarkAllAsRead)]
        public async Task<IActionResult> MarkAllAsRead()
        {
            var currentUser = JwtClaimUltils.GetLoginedUser(_httpContextAccessor);
            var currentUserId = JwtClaimUltils.GetUserId(currentUser);
            
            var result = await _notificationService.MarkAllAsReadAsync(currentUserId);
            return Ok(BaseResponse.OkResponseDto(result));
        }
        
        [HttpPost]
        [Route(WebApiEndpoint.Notification.CreateCartNotification)]
        public async Task<IActionResult> CreateCartNotification([FromBody] int itemCount)
        {
            var currentUser = JwtClaimUltils.GetLoginedUser(_httpContextAccessor);
            var currentUserId = JwtClaimUltils.GetUserId(currentUser);
            
            _logger.Information("Tạo thông báo giỏ hàng cho người dùng {UserId} với số lượng {ItemCount}", currentUserId, itemCount);
            
            var notification = await _notificationService.CreateCartNotificationAsync(currentUserId, itemCount);
            
            _logger.Information("Đã tạo thông báo giỏ hàng: NotificationId={NotificationId}, Message={Message}", 
                            notification.NotificationId, notification.Message);
            
            await SendNotificationViaSignalR(notification);
            await SendPushNotificationViaFirebase(notification, "CART", itemCount);
            
            return Ok(BaseResponse.OkResponseDto(notification));
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
        private async Task SendPushNotificationViaFirebase(NotificationDto notification, string type = "GENERAL", int? itemCount = null)
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
                Dictionary<string, string> data;
                
                if (type == "CART" && itemCount.HasValue)
                {
                    data = Services.Helper.PushNotificationHelper.CreateCartNotificationData(itemCount.Value);
                    _logger.Information("Đã tạo dữ liệu thông báo giỏ hàng với {ItemCount} mục", itemCount.Value);
                }
                else
                {
                    data = new Dictionary<string, string>
                    {
                        { "notificationId", notification.NotificationId.ToString() },
                        { "type", type },
                        { "message", notification.Message },
                        { "timestamp", DateTimeOffset.UtcNow.AddHours(7).ToUnixTimeSeconds().ToString() }
                    };
                    _logger.Information("Đã tạo dữ liệu thông báo chung loại {Type}", type);
                }
                
                // Gửi thông báo đến từng thiết bị của người dùng
                foreach (var device in deviceList)
                {
                    _logger.Information("Đang gửi push notification đến thiết bị {DeviceToken}", 
                                    device.DeviceToken.Substring(0, Math.Min(10, device.DeviceToken.Length)) + "...");
                    
                    var result = await _firebaseService.SendNotificationAsync(
                        device.DeviceToken,
                        "Thông báo mới",
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
                    "Thông báo mới",
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
