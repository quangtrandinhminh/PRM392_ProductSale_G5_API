using System;
using System.Collections.Generic;

namespace Repositories.Models;

public class BroadcastNotification
{
    public string Message { get; set; }
    public DateTime CreatedAt { get; set; }
    public int RecipientCount { get; set; }
    public List<Notification> NotificationInstances { get; set; }
} 