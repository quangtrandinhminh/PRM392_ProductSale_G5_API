using Microsoft.AspNetCore.SignalR;
using Service.Utils;

namespace PRM_ProductSale_G5.Hubs
{
    public class CustomUserIdProvider : IUserIdProvider
    {
        public string GetUserId(HubConnectionContext connection)
        {
            var userId = JwtClaimUltils.GetUserId(connection.User);
            
            Console.WriteLine($"GetUserId called for connection {connection.ConnectionId}, returning userId: {userId}");
            
            return userId.ToString();
        }
    }
} 