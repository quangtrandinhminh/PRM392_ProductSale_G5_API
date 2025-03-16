using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Service.Utils;
using Services.ApiModels.Notification;

namespace PRM_ProductSale_G5.Hubs;

[Authorize]
public class NotificationHub : Hub
{
    public static readonly Dictionary<int, string> _userConnectionMap = new Dictionary<int, string>();
    
    public override async Task OnConnectedAsync()
    {
        var user = Context.User;
        var userId = JwtClaimUltils.GetUserId(user);
        
        _userConnectionMap[userId] = Context.ConnectionId;
        
        await base.OnConnectedAsync();
    }
    
    public override async Task OnDisconnectedAsync(Exception exception)
    {
        var user = Context.User;
        var userId = JwtClaimUltils.GetUserId(user);
        
        _userConnectionMap.Remove(userId);
        
        await base.OnDisconnectedAsync(exception);
    }
    
    public async Task SendNotification(NotificationDto notification)
    {
        // Gửi thông báo đến người nhận nếu họ đang online
        if (_userConnectionMap.TryGetValue(notification.UserId.Value, out var connectionId))
        {
            await Clients.Client(connectionId).SendAsync("ReceiveNotification", notification);
        }
    }
}