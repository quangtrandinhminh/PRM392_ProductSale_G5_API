// Repositories/Repositories/ChatMessageRepository.cs
using Microsoft.EntityFrameworkCore;
using Repositories.Base;
using Repositories.Extensions;
using Repositories.Models;

namespace Repositories.Repositories;

public interface IChatMessageRepository : IGenericRepository<ChatMessage>
{
    Task<PaginatedList<ChatMessage>> GetChatMessagesByUserIdAsync(int userId, int pageNumber, int pageSize);
    Task<PaginatedList<ChatMessage>> GetChatMessagesBetweenUsersAsync(int userId1, int userId2, int pageNumber, int pageSize);
    Task<int> GetUnreadCountAsync(int userId);
    Task<bool> MarkAsReadAsync(int chatMessageId);
}

public class ChatMessageRepository : GenericRepository<ChatMessage>, IChatMessageRepository
{
    private readonly AppDbContext _context;
    
    public ChatMessageRepository(AppDbContext context) : base(context)
    {
        _context = context;
    }
    
    public async Task<PaginatedList<ChatMessage>> GetChatMessagesByUserIdAsync(int userId, int pageNumber, int pageSize)
    {
        var query = _context.ChatMessages
            .Where(m => m.UserId == userId)
            .OrderByDescending(m => m.SentAt)
            .Include(m => m.User);
            
        return await PaginatedList<ChatMessage>.CreateAsync(query, pageNumber, pageSize);
    }
    
    public async Task<PaginatedList<ChatMessage>> GetChatMessagesBetweenUsersAsync(int userId1, int userId2, int pageNumber, int pageSize)
    {
        // Trong mô hình hiện tại, chúng ta cần mở rộng để hỗ trợ chat giữa hai người dùng
        // Tạm thời, chúng ta có thể lấy tin nhắn của cả hai người dùng
        var query = _context.ChatMessages
            .Where(m => m.UserId == userId1 || m.UserId == userId2)
            .OrderByDescending(m => m.SentAt)
            .Include(m => m.User);
            
        return await PaginatedList<ChatMessage>.CreateAsync(query, pageNumber, pageSize);
    }
    
    public async Task<int> GetUnreadCountAsync(int userId)
    {
        return await _context.ChatMessages
            .CountAsync(m => m.UserId == userId && !m.IsRead);
    }
    
    public async Task<bool> MarkAsReadAsync(int chatMessageId)
    {
        var message = await _context.ChatMessages
            .AsTracking()
            .FirstOrDefaultAsync(m => m.ChatMessageId == chatMessageId);
        
        if (message == null) return false;
        
        message.IsRead = true;
        
        _context.Entry(message).State = EntityState.Modified;
        
        await _context.SaveChangesAsync();
        return true;
    }
}