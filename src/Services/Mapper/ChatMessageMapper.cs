using Repositories.Extensions;
using Repositories.Models;
using Services.ApiModels.Chat;
using Services.ApiModels.PaginatedList;
using System.Collections.Generic;
using System.Linq;

namespace Services.Mapper;

public partial class MapperlyMapper
{
    // ChatMessage mappings
    public ChatMessageDto MapChatMessage(ChatMessage chatMessage)
    {
        if (chatMessage == null) return null;
        
        var dto = new ChatMessageDto
        {
            ChatMessageId = chatMessage.ChatMessageId,
            UserId = chatMessage.UserId,
            ReceiverId = chatMessage.ReceiverId,
            Message = chatMessage.Message,
            SentAt = chatMessage.SentAt,
            IsRead = chatMessage.IsRead
        };
        
        if (chatMessage.User != null)
        {
            dto.Username = chatMessage.User.Username;
        }
        
        if (chatMessage.Receiver != null)
        {
            dto.ReceiverUsername = chatMessage.Receiver.Username;
        }
        
        return dto;
    }
    
    public ChatMessage MapChatMessageUpdate(ChatMessageUpdateRequest request, ChatMessage chatMessage)
    {
        if (request == null || chatMessage == null) return chatMessage;
        
        chatMessage.Message = request.Message;
        return chatMessage;
    }
    
    public PaginatedListResponse<ChatMessageDto> MapChatMessageList(PaginatedList<ChatMessage> paginatedList)
    {
        if (paginatedList == null) return null;
        
        var items = paginatedList.Items.Select(MapChatMessage).ToList();
        
        return new PaginatedListResponse<ChatMessageDto>
        {
            Items = items,
            PageNumber = paginatedList.PageNumber,
            TotalPages = paginatedList.TotalPages,
            TotalCount = paginatedList.TotalCount
        };
    }

    public List<ConversationListItemDto> MapConversationList(List<ChatMessage> lastMessages, int currentUserId)
    {
        if (lastMessages == null) return null;
        
        return lastMessages.Select(m => new ConversationListItemDto
        {
            OtherUserId = m.UserId == currentUserId ? m.ReceiverId.Value : m.UserId.Value,
            OtherUsername = m.UserId == currentUserId ? m.Receiver?.Username : m.User?.Username,
            LastMessage = m.Message,
            LastMessageTime = m.SentAt,
            HasUnreadMessages = m.UserId != currentUserId && !m.IsRead,
            IsSentByMe = m.UserId == currentUserId
        }).ToList();
    }
} 