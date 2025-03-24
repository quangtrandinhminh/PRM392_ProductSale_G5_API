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

namespace PRM_ProductSale_G5.Controllers
{
    [ApiController]
    [Authorize]
    public class NotificationController : ControllerBase
    {
        private readonly INotificationService _notificationService;
        private readonly IHubContext<NotificationHub> _notificationHubContext;
        private readonly IHttpContextAccessor _httpContextAccessor;
        
        public NotificationController(IServiceProvider serviceProvider)
        {
            _notificationService = serviceProvider.GetRequiredService<INotificationService>();
            _notificationHubContext = serviceProvider.GetRequiredService<IHubContext<NotificationHub>>();
            _httpContextAccessor = serviceProvider.GetRequiredService<IHttpContextAccessor>();
          
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
            var notification = await _notificationService.CreateNotificationAsync(request);
            
            // Gửi thông báo qua SignalR sử dụng connectionId từ mapping
            if (request.UserId.HasValue)
            {
                if (NotificationHub._userConnectionMap.TryGetValue(request.UserId.Value, out var connectionId))
                {
                    Console.WriteLine($"Sending notification to user {request.UserId.Value} with connection ID {connectionId}");
                    await _notificationHubContext.Clients.Client(connectionId).SendAsync("ReceiveNotification", notification);
                }
                else
                {
                    Console.WriteLine($"User {request.UserId.Value} is not connected to NotificationHub");
                }
            }
            
            return Ok(BaseResponse.OkResponseDto(notification));
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
            
            var notification = await _notificationService.CreateCartNotificationAsync(currentUserId, itemCount);
            
            // Gửi thông báo qua SignalR sử dụng Clients.User
            await _notificationHubContext.Clients.User(currentUserId.ToString()).SendAsync("ReceiveNotification", notification);
            
            return Ok(BaseResponse.OkResponseDto(notification));
        }
    }
}
