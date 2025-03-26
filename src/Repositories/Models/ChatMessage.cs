using System;
using System.Collections.Generic;

namespace Repositories.Models;

public partial class ChatMessage
{
    public int ChatMessageId { get; set; }

    public int? UserId { get; set; }

    public string? Message { get; set; }

    public DateTime? SentAt { get; set; }

    public bool IsRead { get; set; }

    public virtual User? User { get; set; }
}
