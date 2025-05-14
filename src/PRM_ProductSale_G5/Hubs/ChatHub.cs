using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Service.Utils;
using Services.ApiModels.Chat;

namespace PRM_ProductSale_G5.Hubs;

[Authorize]
public class ChatHub : Hub
{
    private static readonly Dictionary<int, string> _userConnectionMap = new Dictionary<int, string>();
    
    public override async Task OnConnectedAsync()
    {
        var user = Context.User;
        var userId = JwtClaimUltils.GetUserId(user);
        
        _userConnectionMap[userId] = Context.ConnectionId;
        
        await base.OnConnectedAsync();
    }
    
    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var user = Context.User;
        var userId = JwtClaimUltils.GetUserId(user);
        
        _userConnectionMap.Remove(userId);
        
        await base.OnDisconnectedAsync(exception);
    }
    
    public async Task SendMessage(ChatMessageDto message)
    {
        // Gửi tin nhắn đến người nhận nếu họ đang online
        if (_userConnectionMap.TryGetValue(message.UserId.Value, out var connectionId))
        {
            await Clients.Client(connectionId).SendAsync("ReceiveMessage", message);
        }
        
        // Gửi tin nhắn đến tất cả người dùng trong nhóm (nếu có)
        // await Clients.Group(groupName).SendAsync("ReceiveMessage", message);
    }
}