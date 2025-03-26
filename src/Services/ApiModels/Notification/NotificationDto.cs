using System.ComponentModel.DataAnnotations;

namespace Services.ApiModels.Notification;

public class NotificationDto
{
    public int NotificationId { get; set; }
    public int? UserId { get; set; }
    public string Username { get; set; }
    public string Message { get; set; }
    public bool IsRead { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class NotificationCreateRequest
{
    [Required]
    public int? UserId { get; set; }
    
    [Required]
    public string Message { get; set; }
}

public class NotificationUpdateRequest
{
    [Required]
    public int NotificationId { get; set; }
    
    [Required]
    public string Message { get; set; }
}
