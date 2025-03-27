using Repositories.Extensions;
using Repositories.Models;
using Services.ApiModels.Notification;
using Services.ApiModels.PaginatedList;
using System.Collections.Generic;
using System.Linq;

namespace Services.Mapper;

public partial class MapperlyMapper
{
    // Notification mappings
    public NotificationDto MapNotification(Notification notification)
    {
        if (notification == null) return null;
        
        var dto = new NotificationDto
        {
            NotificationId = notification.NotificationId,
            UserId = notification.UserId,
            Message = notification.Message,
            IsRead = notification.IsRead,
            CreatedAt = notification.CreatedAt
        };
        
        if (notification.User != null)
        {
            dto.Username = notification.User.Username;
        }
        
        return dto;
    }
    
    public Notification MapNotificationRequest(NotificationCreateRequest request)
    {
        if (request == null) return null;
        
        return new Notification
        {
            UserId = request.UserId,
            Message = request.Message
        };
    }
    
    public Notification MapNotificationUpdate(NotificationUpdateRequest request, Notification notification)
    {
        if (request == null || notification == null) return notification;
        
        notification.Message = request.Message;
        return notification;
    }
    
    public PaginatedListResponse<NotificationDto> MapNotificationList(PaginatedList<Notification> paginatedList)
    {
        if (paginatedList == null) return null;
        
        var items = paginatedList.Items.Select(MapNotification).ToList();
        
        return new PaginatedListResponse<NotificationDto>
        {
            Items = items,
            PageNumber = paginatedList.PageNumber,
            TotalPages = paginatedList.TotalPages,
            TotalCount = paginatedList.TotalCount
        };
    }
    
    public BroadcastNotificationDto MapBroadcastNotification(BroadcastNotification broadcastNotification)
    {
        if (broadcastNotification == null) return null;
        
        var dto = new BroadcastNotificationDto
        {
            Message = broadcastNotification.Message,
            CreatedAt = broadcastNotification.CreatedAt,
            RecipientCount = broadcastNotification.RecipientCount,
            NotificationInstances = broadcastNotification.NotificationInstances?.Select(MapNotification).ToList() ?? new List<NotificationDto>()
        };
        
        return dto;
    }
    
    public PaginatedListResponse<BroadcastNotificationDto> MapBroadcastNotificationList(PaginatedList<BroadcastNotification> paginatedList)
    {
        if (paginatedList == null) return null;
        
        var items = paginatedList.Items.Select(MapBroadcastNotification).ToList();
        
        return new PaginatedListResponse<BroadcastNotificationDto>
        {
            Items = items,
            PageNumber = paginatedList.PageNumber,
            TotalPages = paginatedList.TotalPages,
            TotalCount = paginatedList.TotalCount
        };
    }
} 