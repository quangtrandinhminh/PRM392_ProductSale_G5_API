using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Repositories.Repositories;
using Serilog;
using Service.Utils;
using Services.ApiModels.Chat;
using Services.ApiModels.PaginatedList;
using Services.Constants;
using Services.Exceptions;
using Services.Mapper;
using Repositories.Repositories;
using Repositories.Models;
using System.Linq;

namespace Services.Services;

public interface IChatMessageService
{
    Task<PaginatedListResponse<ChatMessageDto>> GetChatMessagesByUserIdAsync(int userId, int pageNumber, int pageSize);
    Task<PaginatedListResponse<ChatMessageDto>> GetChatMessagesBetweenUsersAsync(int userId1, int userId2, int pageNumber, int pageSize);
    Task<ChatMessageDto> GetChatMessageByIdAsync(int id);
    Task<ChatMessageDto> CreateChatMessageAsync(int userId, ChatMessageCreateRequest request);
    Task<int> UpdateChatMessageAsync(ChatMessageUpdateRequest request);
    Task<int> DeleteChatMessageAsync(int id);
    Task<int> GetUnreadCountAsync(int userId);
    Task<bool> MarkAsReadAsync(int chatMessageId);
    Task<ConversationInfoDto> GetConversationInfoAsync(int currentUserId, int otherUserId);
    Task<List<ConversationListItemDto>> GetConversationListAsync(int userId);
}

public class ChatMessageService : IChatMessageService
{
    private readonly IChatMessageRepository _chatMessageRepository;
    private readonly ILogger _logger;
    private readonly MapperlyMapper _mapper;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IUserRepository _userRepository;
    
    public ChatMessageService(IServiceProvider serviceProvider)
    {
        _chatMessageRepository = serviceProvider.GetRequiredService<IChatMessageRepository>();
        _logger = serviceProvider.GetRequiredService<ILogger>();
        _mapper = serviceProvider.GetRequiredService<MapperlyMapper>();
        _httpContextAccessor = serviceProvider.GetRequiredService<IHttpContextAccessor>();
        _userRepository = serviceProvider.GetRequiredService<IUserRepository>();
    }
    
    public async Task<PaginatedListResponse<ChatMessageDto>> GetChatMessagesByUserIdAsync(int userId, int pageNumber, int pageSize)
    {
        _logger.Information($"Getting chat messages for user {userId}, page {pageNumber}, size {pageSize}");
        var messages = await _chatMessageRepository.GetChatMessagesByUserIdAsync(userId, pageNumber, pageSize);
        return _mapper.MapChatMessageList(messages);
    }
    
    public async Task<PaginatedListResponse<ChatMessageDto>> GetChatMessagesBetweenUsersAsync(int userId1, int userId2, int pageNumber, int pageSize)
    {
        _logger.Information($"Getting chat messages between users {userId1} and {userId2}, page {pageNumber}, size {pageSize}");
        var messages = await _chatMessageRepository.GetChatMessagesBetweenUsersAsync(userId1, userId2, pageNumber, pageSize);
        return _mapper.MapChatMessageList(messages);
    }
    
    public async Task<ChatMessageDto> GetChatMessageByIdAsync(int id)
    {
        _logger.Information($"Getting chat message with id {id}");
        var message = await _chatMessageRepository.GetSingleAsync(x => x.ChatMessageId == id, x => x.User);
        if (message == null)
        {
            throw new AppException(ResponseCodeConstants.BAD_REQUEST, 
                ResponseMessageConstraintsChat.NOT_FOUND, StatusCodes.Status404NotFound);
        }
        
        return _mapper.MapChatMessage(message);
    }
    
    public async Task<ChatMessageDto> CreateChatMessageAsync(int userId, ChatMessageCreateRequest request)
    {
        _logger.Information($"Creating chat message from user {userId} to user {request.ReceiverId}");
        
        var message = new ChatMessage
        {
            UserId = userId,
            ReceiverId = request.ReceiverId,
            Message = request.Message,
            SentAt = DateTime.UtcNow.AddHours(7),
            IsRead = false
        };
        
        await _chatMessageRepository.AddAsync(message);
        await _chatMessageRepository.SaveChangeAsync();
        
        // Lấy tin nhắn vừa tạo kèm thông tin người gửi và người nhận
        var createdMessage = await _chatMessageRepository.GetSingleAsync(
            x => x.ChatMessageId == message.ChatMessageId, 
            x => x.User,
            x => x.Receiver);
        
        return _mapper.MapChatMessage(createdMessage);
    }
    
    public async Task<int> UpdateChatMessageAsync(ChatMessageUpdateRequest request)
    {
        _logger.Information($"Updating chat message {request.ChatMessageId}");
        
        var message = await _chatMessageRepository.GetSingleAsync(x => x.ChatMessageId == request.ChatMessageId);
        if (message == null)
        {
            throw new AppException(ResponseCodeConstants.BAD_REQUEST,
                ResponseMessageConstraintsChat.NOT_FOUND, StatusCodes.Status404NotFound);
        }
        
        // Kiểm tra quyền chỉnh sửa
        var currentUser = JwtClaimUltils.GetLoginedUser(_httpContextAccessor);
        var currentUserId = JwtClaimUltils.GetUserId(currentUser);
        
        if (message.UserId != currentUserId)
        {
            throw new AppException(ResponseCodeConstants.FORBIDDEN, 
               ResponseMessageConstraintsChat.FORBIDDEN_UPDATE, StatusCodes.Status403Forbidden);
        }
        
        _mapper.MapChatMessageUpdate(request, message);
        _chatMessageRepository.Update(message);
        
        return await _chatMessageRepository.SaveChangeAsync();
    }
    
    public async Task<int> DeleteChatMessageAsync(int id)
    {
        _logger.Information($"Deleting chat message {id}");
        
        var message = await _chatMessageRepository.GetSingleAsync(x => x.ChatMessageId == id);
        if (message == null)
        {
            throw new AppException(ResponseCodeConstants.BAD_REQUEST,
                ResponseMessageConstraintsChat.NOT_FOUND, StatusCodes.Status404NotFound);
        }
        
        // Kiểm tra quyền xóa
        var currentUser = JwtClaimUltils.GetLoginedUser(_httpContextAccessor);
        var currentUserId = JwtClaimUltils.GetUserId(currentUser);
        
        if (message.UserId != currentUserId)
        {
            throw new AppException(ResponseCodeConstants.FORBIDDEN,
                ResponseMessageConstraintsChat.FORBIDDEN_DELETE, StatusCodes.Status403Forbidden);
        }
        
        _chatMessageRepository.Remove(message);
        
        return await _chatMessageRepository.SaveChangeAsync();
    }
    
    public async Task<ConversationInfoDto> GetConversationInfoAsync(int currentUserId, int otherUserId)
    {
        _logger.Information($"Getting conversation info between users {currentUserId} and {otherUserId}");
        
        var currentUser = await _userRepository.GetSingleAsync(u => u.UserId == currentUserId);
        var otherUser = await _userRepository.GetSingleAsync(u => u.UserId == otherUserId);
        
        if (currentUser == null || otherUser == null)
        {
            throw new AppException(ResponseCodeConstants.BAD_REQUEST, 
                ResponseMessageConstraintsChat.NOT_FOUND, StatusCodes.Status404NotFound);
        }
        
        return new ConversationInfoDto
        {
            CurrentUserId = currentUserId,
            CurrentUsername = currentUser.Username,
            OtherUserId = otherUserId,
            OtherUsername = otherUser.Username
        };
    }
    
    public async Task<List<ConversationListItemDto>> GetConversationListAsync(int userId)
    {
        _logger.Information($"Getting conversation list for user {userId}");
        
        // Lấy danh sách tất cả người dùng có trò chuyện với người dùng hiện tại
        var conversationUsers = await _chatMessageRepository.GetConversationUsersAsync(userId);
        var lastMessages = new List<ChatMessage>();
        
        foreach (var otherUserId in conversationUsers)
        {
            // Lấy tin nhắn cuối cùng giữa hai người dùng
            var lastMessage = await _chatMessageRepository.GetLastMessageAsync(userId, otherUserId);
            if (lastMessage != null)
            {
                lastMessages.Add(lastMessage);
            }
        }
        
        // Sử dụng mapper để chuyển đổi và sắp xếp theo thời gian (mới nhất lên đầu)
        var result = _mapper.MapConversationList(lastMessages, userId);
        return result.OrderByDescending(c => c.LastMessageTime).ToList();
    }
    
    public async Task<int> GetUnreadCountAsync(int userId)
    {
        return await _chatMessageRepository.GetUnreadCountAsync(userId);
    }
    
    public async Task<bool> MarkAsReadAsync(int chatMessageId)
    {
        return await _chatMessageRepository.MarkAsReadAsync(chatMessageId);
    }
}