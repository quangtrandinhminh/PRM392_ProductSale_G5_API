namespace Services.Helper;

public static class PushNotificationHelper
{
    public static Dictionary<string, string> CreateCartNotificationData(int itemCount)
    {
        return new Dictionary<string, string>
        {
            { "type", "CART" },
            { "itemCount", itemCount.ToString() },
            { "timestamp", DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString() }
        };
    }
    
    public static Dictionary<string, string> CreateChatNotificationData(int chatMessageId, int userId, string message)
    {
        return new Dictionary<string, string>
        {
            { "type", "CHAT" },
            { "chatMessageId", chatMessageId.ToString() },
            { "userId", userId.ToString() },
            { "message", message },
            { "timestamp", DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString() }
        };
    }
    
    public static Dictionary<string, string> CreateOrderNotificationData(int orderId, string status)
    {
        return new Dictionary<string, string>
        {
            { "type", "ORDER" },
            { "orderId", orderId.ToString() },
            { "status", status },
            { "timestamp", DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString() }
        };
    }
}
