using System.ComponentModel.DataAnnotations;

namespace Services.ApiModels.Chat;

public class ChatMessageDto
{
    public int ChatMessageId { get; set; }
    public int? UserId { get; set; }
    public string Username { get; set; }
    public string Message { get; set; }
    public DateTime SentAt { get; set; }
    public bool IsRead { get; set; }
}

public class ChatMessageCreateRequest
{
    [Required]
    public int? UserId { get; set; }
    
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

