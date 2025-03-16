// Repositories/Repositories/NotificationRepository.cs
using Microsoft.EntityFrameworkCore;
using Repositories.Base;
using Repositories.Extensions;
using Repositories.Models;

namespace Repositories.Repositories;

public interface INotificationRepository : IGenericRepository<Notification>
{
    Task<PaginatedList<Notification>> GetNotificationsByUserIdAsync(int userId, int pageNumber, int pageSize);
    Task<int> GetUnreadCountAsync(int userId);
    Task<bool> MarkAsReadAsync(int notificationId);
    Task<bool> MarkAllAsReadAsync(int userId);
}

public class NotificationRepository : GenericRepository<Notification>, INotificationRepository
{
    private readonly AppDbContext _context;
    
    public NotificationRepository(AppDbContext context) : base(context)
    {
        _context = context;
    }
    
    public async Task<PaginatedList<Notification>> GetNotificationsByUserIdAsync(int userId, int pageNumber, int pageSize)
    {
        var query = _context.Notifications
            .Where(n => n.UserId == userId)
            .OrderByDescending(n => n.CreatedAt)
            .Include(n => n.User);
            
        return await PaginatedList<Notification>.CreateAsync(query, pageNumber, pageSize);
    }
    
    public async Task<int> GetUnreadCountAsync(int userId)
    {
        return await _context.Notifications
            .CountAsync(n => n.UserId == userId && !n.IsRead);
    }
    
    public async Task<bool> MarkAsReadAsync(int notificationId)
    {
        var notification = await _context.Notifications.FindAsync(notificationId);
        if (notification == null) return false;
        
        notification.IsRead = true;
        await _context.SaveChangesAsync();
        return true;
    }
    
    public async Task<bool> MarkAllAsReadAsync(int userId)
    {
        var notifications = await _context.Notifications
            .Where(n => n.UserId == userId && !n.IsRead)
            .ToListAsync();
            
        if (!notifications.Any()) return false;
        
        foreach (var notification in notifications)
        {
            notification.IsRead = true;
        }
        
        await _context.SaveChangesAsync();
        return true;
    }
}