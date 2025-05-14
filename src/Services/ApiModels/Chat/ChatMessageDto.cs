using System.ComponentModel.DataAnnotations;

namespace Services.ApiModels.Chat;

public class ChatMessageDto
{
    public int ChatMessageId { get; set; }
    public int? UserId { get; set; }
    public string Username { get; set; }
    public int? ReceiverId { get; set; }
    public string ReceiverUsername { get; set; }
    public string Message { get; set; }
    public DateTime SentAt { get; set; }
    public bool IsRead { get; set; }
}

public class ChatMessageCreateRequest
{
    [Required]
    public int? ReceiverId { get; set; }
    
    [Required]
    public string Message { get; set; }
}

public class ChatMessageUpdateRequest
{
    [Required]
    public int ChatMessageId { get; set; }
    
    [Required]
    public string Message { get; set; }
}

public class ConversationInfoDto
{
    public int CurrentUserId { get; set; }
    public string CurrentUsername { get; set; }
    public int OtherUserId { get; set; }
    public string OtherUsername { get; set; }
}

public class ConversationListItemDto
{
    public int OtherUserId { get; set; }
    public string OtherUsername { get; set; }
    public string LastMessage { get; set; }
    public DateTime LastMessageTime { get; set; }
    public bool HasUnreadMessages { get; set; }
    public bool IsSentByMe { get; set; }
}

