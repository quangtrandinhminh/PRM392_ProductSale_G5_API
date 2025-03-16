using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Repositories.Extensions;
using Repositories.Models;
using Repositories.Repositories;
using Serilog;
using Service.Utils;
using Services.ApiModels;
using Services.ApiModels.Notification;
using Services.ApiModels.PaginatedList;
using Services.Constants;
using Services.Exceptions;
using Services.Mapper;

namespace Services.Services;

public interface INotificationService
{
    Task<PaginatedListResponse<NotificationDto>> GetNotificationsByUserIdAsync(int userId, int pageNumber, int pageSize);
    Task<NotificationDto> GetNotificationByIdAsync(int id);
    Task<NotificationDto> CreateNotificationAsync(NotificationCreateRequest request);
    Task<int> UpdateNotificationAsync(NotificationUpdateRequest request);
    Task<int> DeleteNotificationAsync(int id);
    Task<int> GetUnreadCountAsync(int userId);
    Task<bool> MarkAsReadAsync(int notificationId);
    Task<bool> MarkAllAsReadAsync(int userId);
    Task<NotificationDto> CreateCartNotificationAsync(int userId, int itemCount);
}

public class NotificationService : INotificationService
{
    private readonly INotificationRepository _notificationRepository;
    private readonly ILogger _logger;
    private readonly MapperlyMapper _mapper;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IFirebaseService _firebaseService;
    private readonly IUserDeviceRepository _userDeviceRepository;
    
    public NotificationService(IServiceProvider serviceProvider)
    {
        _notificationRepository = serviceProvider.GetRequiredService<INotificationRepository>();
        _logger = serviceProvider.GetRequiredService<ILogger>();
        _mapper = serviceProvider.GetRequiredService<MapperlyMapper>();
        _httpContextAccessor = serviceProvider.GetRequiredService<IHttpContextAccessor>();
        _firebaseService = serviceProvider.GetRequiredService<IFirebaseService>();
        _userDeviceRepository = serviceProvider.GetRequiredService<IUserDeviceRepository>();
    }
    
    public async Task<PaginatedListResponse<NotificationDto>> GetNotificationsByUserIdAsync(int userId, int pageNumber, int pageSize)
    {
        _logger.Information($"Getting notifications for user {userId}, page {pageNumber}, size {pageSize}");
        var notifications = await _notificationRepository.GetNotificationsByUserIdAsync(userId, pageNumber, pageSize);
        return _mapper.Map(notifications);
    }
    
    public async Task<NotificationDto> GetNotificationByIdAsync(int id)
    {
        _logger.Information($"Getting notification with id {id}");
        var notification = await _notificationRepository.GetSingleAsync(x => x.NotificationId == id, x => x.User);
        if (notification == null)
        {
            throw new AppException(ResponseCodeConstants.BAD_REQUEST, 
                "Không tìm thấy thông báo", StatusCodes.Status404NotFound);
        }
        
        return _mapper.Map(notification);
    }
    
    public async Task<NotificationDto> CreateNotificationAsync(NotificationCreateRequest request)
    {
        _logger.Information($"Creating notification for user {request.UserId}");
        
        var notification = _mapper.Map(request);
        notification.CreatedAt = DateTime.UtcNow;
        notification.IsRead = false;
        
        await _notificationRepository.AddAsync(notification);
        await _notificationRepository.SaveChangeAsync();
        
        var result = _mapper.Map(notification);
        
        // Gửi thông báo đẩy qua Firebase
        await SendPushNotificationAsync(notification);
        
        return result;
    }
    
    private async Task SendPushNotificationAsync(Notification notification)
    {
        if (notification.UserId.HasValue)
        {
            var userDevices = await _userDeviceRepository.GetUserDevicesAsync(notification.UserId.Value);
            
            foreach (var device in userDevices)
            {
                var data = new Dictionary<string, string>
                {
                    { "notificationId", notification.NotificationId.ToString() },
                    { "type", "GENERAL" },
                    { "timestamp", DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString() }
                };
                
                await _firebaseService.SendNotificationAsync(
                    device.DeviceToken,
                    "Thông báo mới",
                    notification.Message,
                    data
                );
            }
        }
    }
    
    public async Task<int> UpdateNotificationAsync(NotificationUpdateRequest request)
    {
        _logger.Information($"Updating notification {request.NotificationId}");
        
        var notification = await _notificationRepository.GetSingleAsync(x => x.NotificationId == request.NotificationId);
        if (notification == null)
        {
            throw new AppException(ResponseCodeConstants.BAD_REQUEST, 
                "Không tìm thấy thông báo", StatusCodes.Status404NotFound);
        }
        
        // Kiểm tra quyền chỉnh sửa (chỉ admin mới có quyền)
        var currentUser = JwtClaimUltils.GetLoginedUser(_httpContextAccessor);
        var roles = JwtClaimUltils.GetUserRole(currentUser);
        
        if (!roles.Contains("Admin"))
        {
            throw new AppException(ResponseCodeConstants.FORBIDDEN, 
                "Bạn không có quyền chỉnh sửa thông báo này", StatusCodes.Status403Forbidden);
        }
        
        _mapper.Map(request, notification);
        _notificationRepository.Update(notification);
        
        return await _notificationRepository.SaveChangeAsync();
    }
    
    public async Task<int> DeleteNotificationAsync(int id)
    {
        _logger.Information($"Deleting notification {id}");
        
        var notification = await _notificationRepository.GetSingleAsync(x => x.NotificationId == id);
        if (notification == null)
        {
            throw new AppException(ResponseCodeConstants.BAD_REQUEST, 
                "Không tìm thấy thông báo", StatusCodes.Status404NotFound);
        }
        
        // Kiểm tra quyền xóa (chỉ admin hoặc chủ sở hữu mới có quyền)
        var currentUser = JwtClaimUltils.GetLoginedUser(_httpContextAccessor);
        var currentUserId = JwtClaimUltils.GetUserId(currentUser);
        var roles = JwtClaimUltils.GetUserRole(currentUser);
        
        if (notification.UserId != currentUserId && !roles.Contains("Admin"))
        {
            throw new AppException(ResponseCodeConstants.FORBIDDEN, 
                "Bạn không có quyền xóa thông báo này", StatusCodes.Status403Forbidden);
        }
        
        _notificationRepository.Remove(notification);
        
        return await _notificationRepository.SaveChangeAsync();
    }
    
    public async Task<int> GetUnreadCountAsync(int userId)
    {
        return await _notificationRepository.GetUnreadCountAsync(userId);
    }
    
    public async Task<bool> MarkAsReadAsync(int notificationId)
    {
        return await _notificationRepository.MarkAsReadAsync(notificationId);
    }
    
    public async Task<bool> MarkAllAsReadAsync(int userId)
    {
        return await _notificationRepository.MarkAllAsReadAsync(userId);
    }
    
    public async Task<NotificationDto> CreateCartNotificationAsync(int userId, int itemCount)
    {
        var request = new NotificationCreateRequest
        {
            UserId = userId,
            Message = $"Bạn có {itemCount} sản phẩm trong giỏ hàng"
        };
        
        var notification = await CreateNotificationAsync(request);
        
        // Gửi thông báo đẩy đặc biệt cho giỏ hàng
        var userDevices = await _userDeviceRepository.GetUserDevicesAsync(userId);
        
        foreach (var device in userDevices)
        {
            var data = new Dictionary<string, string>
            {
                { "notificationId", notification.NotificationId.ToString() },
                { "type", "CART" },
                { "itemCount", itemCount.ToString() },
                { "timestamp", DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString() }
            };
            
            await _firebaseService.SendNotificationAsync(
                device.DeviceToken,
                "Giỏ hàng của bạn",
                notification.Message,
                data
            );
        }
        
        return notification;
    }
}