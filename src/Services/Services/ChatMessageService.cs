using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Repositories.Extensions;
using Repositories.Models;
using Repositories.Repositories;
using Serilog;
using Service.Utils;
using Services.ApiModels;
using Services.ApiModels.Chat;
using Services.ApiModels.PaginatedList;
using Services.Constants;
using Services.Exceptions;
using Services.Mapper;

namespace Services.Services;

public interface IChatMessageService
{
    Task<PaginatedListResponse<ChatMessageDto>> GetChatMessagesByUserIdAsync(int userId, int pageNumber, int pageSize);
    Task<PaginatedListResponse<ChatMessageDto>> GetChatMessagesBetweenUsersAsync(int userId1, int userId2, int pageNumber, int pageSize);
    Task<ChatMessageDto> GetChatMessageByIdAsync(int id);
    Task<ChatMessageDto> CreateChatMessageAsync(ChatMessageCreateRequest request);
    Task<int> UpdateChatMessageAsync(ChatMessageUpdateRequest request);
    Task<int> DeleteChatMessageAsync(int id);
    Task<int> GetUnreadCountAsync(int userId);
    Task<bool> MarkAsReadAsync(int chatMessageId);
}

public class ChatMessageService : IChatMessageService
{
    private readonly IChatMessageRepository _chatMessageRepository;
    private readonly ILogger _logger;
    private readonly MapperlyMapper _mapper;
    private readonly IHttpContextAccessor _httpContextAccessor;
    
    public ChatMessageService(IServiceProvider serviceProvider)
    {
        _chatMessageRepository = serviceProvider.GetRequiredService<IChatMessageRepository>();
        _logger = serviceProvider.GetRequiredService<ILogger>();
        _mapper = serviceProvider.GetRequiredService<MapperlyMapper>();
        _httpContextAccessor = serviceProvider.GetRequiredService<IHttpContextAccessor>();
    }
    
    public async Task<PaginatedListResponse<ChatMessageDto>> GetChatMessagesByUserIdAsync(int userId, int pageNumber, int pageSize)
    {
        _logger.Information($"Getting chat messages for user {userId}, page {pageNumber}, size {pageSize}");
        var messages = await _chatMessageRepository.GetChatMessagesByUserIdAsync(userId, pageNumber, pageSize);
        return _mapper.Map(messages);
    }
    
    public async Task<PaginatedListResponse<ChatMessageDto>> GetChatMessagesBetweenUsersAsync(int userId1, int userId2, int pageNumber, int pageSize)
    {
        _logger.Information($"Getting chat messages between users {userId1} and {userId2}, page {pageNumber}, size {pageSize}");
        var messages = await _chatMessageRepository.GetChatMessagesBetweenUsersAsync(userId1, userId2, pageNumber, pageSize);
        return _mapper.Map(messages);
    }
    
    public async Task<ChatMessageDto> GetChatMessageByIdAsync(int id)
    {
        _logger.Information($"Getting chat message with id {id}");
        var message = await _chatMessageRepository.GetSingleAsync(x => x.ChatMessageId == id, x => x.User);
        if (message == null)
        {
            throw new AppException(ResponseCodeConstants.BAD_REQUEST, 
                "Không tìm thấy tin nhắn", StatusCodes.Status404NotFound);
        }
        
        return _mapper.Map(message);
    }
    
    public async Task<ChatMessageDto> CreateChatMessageAsync(ChatMessageCreateRequest request)
    {
        _logger.Information($"Creating chat message for user {request.UserId}");
        
        var message = _mapper.Map(request);
        message.SentAt = DateTime.UtcNow;
        
        await _chatMessageRepository.AddAsync(message);
        await _chatMessageRepository.SaveChangeAsync();
        
        return _mapper.Map(message);
    }
    
    public async Task<int> UpdateChatMessageAsync(ChatMessageUpdateRequest request)
    {
        _logger.Information($"Updating chat message {request.ChatMessageId}");
        
        var message = await _chatMessageRepository.GetSingleAsync(x => x.ChatMessageId == request.ChatMessageId);
        if (message == null)
        {
            throw new AppException(ResponseCodeConstants.BAD_REQUEST, 
                "Không tìm thấy tin nhắn", StatusCodes.Status404NotFound);
        }
        
        // Kiểm tra quyền chỉnh sửa
        var currentUser = JwtClaimUltils.GetLoginedUser(_httpContextAccessor);
        var currentUserId = JwtClaimUltils.GetUserId(currentUser);
        
        if (message.UserId != currentUserId)
        {
            throw new AppException(ResponseCodeConstants.FORBIDDEN, 
                "Bạn không có quyền chỉnh sửa tin nhắn này", StatusCodes.Status403Forbidden);
        }
        
        _mapper.Map(request, message);
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
                "Không tìm thấy tin nhắn", StatusCodes.Status404NotFound);
        }
        
        // Kiểm tra quyền xóa
        var currentUser = JwtClaimUltils.GetLoginedUser(_httpContextAccessor);
        var currentUserId = JwtClaimUltils.GetUserId(currentUser);
        
        if (message.UserId != currentUserId)
        {
            throw new AppException(ResponseCodeConstants.FORBIDDEN, 
                "Bạn không có quyền xóa tin nhắn này", StatusCodes.Status403Forbidden);
        }
        
        _chatMessageRepository.Remove(message);
        
        return await _chatMessageRepository.SaveChangeAsync();
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