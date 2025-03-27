// Repositories/Repositories/ChatMessageRepository.cs
using Microsoft.EntityFrameworkCore;
using Repositories.Base;
using Repositories.Extensions;
using Repositories.Models;
using System.Collections.Generic;
using System.Linq;

namespace Repositories.Repositories;

public interface IChatMessageRepository : IGenericRepository<ChatMessage>
{
    Task<PaginatedList<ChatMessage>> GetChatMessagesByUserIdAsync(int userId, int pageNumber, int pageSize);
    Task<PaginatedList<ChatMessage>> GetChatMessagesBetweenUsersAsync(int userId1, int userId2, int pageNumber, int pageSize);
    Task<int> GetUnreadCountAsync(int userId);
    Task<bool> MarkAsReadAsync(int chatMessageId);
    Task<List<int>> GetConversationUsersAsync(int userId);
    Task<ChatMessage> GetLastMessageAsync(int userId1, int userId2);
    Task<bool> HasUnreadMessagesAsync(int userId, int otherUserId);
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
            .Where(m => m.UserId == userId || m.ReceiverId == userId)
            .OrderByDescending(m => m.SentAt)
            .Include(m => m.User)
            .Include(m => m.Receiver);
            
        return await PaginatedList<ChatMessage>.CreateAsync(query, pageNumber, pageSize);
    }
    
    public async Task<PaginatedList<ChatMessage>> GetChatMessagesBetweenUsersAsync(int userId1, int userId2, int pageNumber, int pageSize)
    {
        // Sử dụng cả UserId và ReceiverId để lấy tin nhắn giữa hai người dùng
        var query = _context.ChatMessages
            .Where(m => (m.UserId == userId1 && m.ReceiverId == userId2) || 
                       (m.UserId == userId2 && m.ReceiverId == userId1))
            .OrderByDescending(m => m.SentAt)
            .Include(m => m.User)
            .Include(m => m.Receiver);
            
        return await PaginatedList<ChatMessage>.CreateAsync(query, pageNumber, pageSize);
    }
    
    public async Task<int> GetUnreadCountAsync(int userId)
    {
        return await _context.ChatMessages
            .CountAsync(m => m.ReceiverId == userId && !m.IsRead);
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
    
    public async Task<List<int>> GetConversationUsersAsync(int userId)
    {
        // Lấy danh sách tất cả người dùng có tin nhắn trao đổi với userId
        var senderIds = await _context.ChatMessages
            .Where(m => m.ReceiverId == userId)
            .Select(m => m.UserId.Value)
            .Distinct()
            .ToListAsync();
        
        var receiverIds = await _context.ChatMessages
            .Where(m => m.UserId == userId)
            .Select(m => m.ReceiverId.Value)
            .Distinct()
            .ToListAsync();
        
        // Kết hợp cả hai danh sách và loại bỏ trùng lặp
        return senderIds.Union(receiverIds).ToList();
    }
    
    public async Task<ChatMessage> GetLastMessageAsync(int userId1, int userId2)
    {
        // Lấy tin nhắn cuối cùng giữa 2 người dùng, xét cả chiều gửi và nhận
        return await _context.ChatMessages
            .Where(m => (m.UserId == userId1 && m.ReceiverId == userId2) || 
                       (m.UserId == userId2 && m.ReceiverId == userId1))
            .OrderByDescending(m => m.SentAt)
            .Include(m => m.User)
            .Include(m => m.Receiver)
            .FirstOrDefaultAsync();
    }
    
    public async Task<bool> HasUnreadMessagesAsync(int userId, int otherUserId)
    {
        // Kiểm tra xem có tin nhắn chưa đọc từ otherUserId gửi đến userId không
        return await _context.ChatMessages
            .AnyAsync(m => m.UserId == otherUserId && m.ReceiverId == userId && !m.IsRead);
    }
}