using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using PRM_ProductSale_G5.Hubs;
using Service.Utils;
using Services.ApiModels;
using Services.ApiModels.Chat;
using Services.ApiModels.PaginatedList;
using Services.Constants;
using Services.Services;

namespace PRM_ProductSale_G5.Controllers;

[ApiController]
[Authorize]
public class ChatMessageController : ControllerBase
{
    private readonly IChatMessageService _chatMessageService;
    private readonly IHubContext<ChatHub> _chatHubContext;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IFirebaseService _firebaseService;
    private readonly IUserDeviceService _userDeviceService;
    private readonly IUserService _userService;
    
    public ChatMessageController(IServiceProvider serviceProvider)
    {
        _chatMessageService = serviceProvider.GetRequiredService<IChatMessageService>();
        _chatHubContext = serviceProvider.GetRequiredService<IHubContext<ChatHub>>();
        _httpContextAccessor = serviceProvider.GetRequiredService<IHttpContextAccessor>();
        _firebaseService = serviceProvider.GetRequiredService<IFirebaseService>();
        _userDeviceService = serviceProvider.GetRequiredService<IUserDeviceService>();
        _userService = serviceProvider.GetRequiredService<IUserService>();
    }
    
    [HttpGet]
    [Route(WebApiEndpoint.ChatMessage.GetChatMessages)]
    public async Task<IActionResult> GetChatMessages([FromQuery] PaginatedListRequest request)
    {
        var currentUser = JwtClaimUltils.GetLoginedUser(_httpContextAccessor);
        var currentUserId = JwtClaimUltils.GetUserId(currentUser);
        
        var messages = await _chatMessageService.GetChatMessagesByUserIdAsync(
            currentUserId, request.PageNumber, request.PageSize);
            
        return Ok(BaseResponse.OkResponseDto(messages));
    }
    
    [HttpGet]
    [Route(WebApiEndpoint.ChatMessage.GetChatMessagesBetweenUsers)]
    public async Task<IActionResult> GetChatMessagesBetweenUsers(
        [FromRoute] int otherUserId, [FromQuery] PaginatedListRequest request)
    {
        var currentUser = JwtClaimUltils.GetLoginedUser(_httpContextAccessor);
        var currentUserId = JwtClaimUltils.GetUserId(currentUser);
        
        var messages = await _chatMessageService.GetChatMessagesBetweenUsersAsync(
            currentUserId, otherUserId, request.PageNumber, request.PageSize);
            
        return Ok(BaseResponse.OkResponseDto(messages));
    }
    
    [HttpGet]
    [Route(WebApiEndpoint.ChatMessage.GetChatMessage)]
    public async Task<IActionResult> GetChatMessage([FromRoute] int id)
    {
        var message = await _chatMessageService.GetChatMessageByIdAsync(id);
        return Ok(BaseResponse.OkResponseDto(message));
    }
    
    [HttpPost]
    [Route(WebApiEndpoint.ChatMessage.CreateChatMessage)]
    public async Task<IActionResult> CreateChatMessage([FromBody] ChatMessageCreateRequest request)
    {
        var currentUser = JwtClaimUltils.GetLoginedUser(_httpContextAccessor);
        var currentUserId = JwtClaimUltils.GetUserId(currentUser);
        
        // Đảm bảo tin nhắn có người nhận
        if (!request.ReceiverId.HasValue)
        {
            return BadRequest(new { Message = "Vui lòng chọn người nhận tin nhắn" });
        }
        
        var message = await _chatMessageService.CreateChatMessageAsync(currentUserId, request);
        
        // Gửi tin nhắn qua SignalR cho người nhận
        await _chatHubContext.Clients.User(request.ReceiverId.Value.ToString())
            .SendAsync("ReceiveMessage", message);
            
        // Gửi Firebase Push Notification
        try 
        {
            // Lấy thông tin người gửi
            var senderInfo = await _userService.GetUserByIdAsync(currentUserId);
            
            // Lấy danh sách thiết bị của người nhận
            var userDevices = await _userDeviceService.GetUserDevicesAsync(request.ReceiverId.Value);
            if (userDevices != null && userDevices.Any())
            {
                // Chuẩn bị dữ liệu
                var data = new Dictionary<string, string>
                {
                    { "messageId", message.ChatMessageId.ToString() },
                    { "senderId", currentUserId.ToString() },
                    { "type", "chat" }
                };
                
                // Gửi thông báo đến tất cả thiết bị của người nhận
                foreach (var device in userDevices)
                {
                    await _firebaseService.SendNotificationAsync(
                        device.DeviceToken,
                        $"Tin nhắn mới từ {senderInfo.Username}",
                        message.Message,
                        data
                    );
                }
            }
        }
        catch (Exception ex)
        {
            // Log lỗi nhưng không ngăn việc trả về tin nhắn
            Console.WriteLine($"Lỗi khi gửi Firebase notification: {ex.Message}");
        }
        
        return Ok(BaseResponse.OkResponseDto(message));
    }
    
    [HttpPut]
    [Route(WebApiEndpoint.ChatMessage.UpdateChatMessage)]
    public async Task<IActionResult> UpdateChatMessage([FromBody] ChatMessageUpdateRequest request)
    {
        var result = await _chatMessageService.UpdateChatMessageAsync(request);
        return Ok(BaseResponse.OkResponseDto(result));
    }
    
    [HttpDelete]
    [Route(WebApiEndpoint.ChatMessage.DeleteChatMessage)]
    public async Task<IActionResult> DeleteChatMessage([FromRoute] int id)
    {
        var result = await _chatMessageService.DeleteChatMessageAsync(id);
        return Ok(BaseResponse.OkResponseDto(result));
    }
    
    [HttpGet]
    [Route(WebApiEndpoint.ChatMessage.GetUnreadCount)]
    public async Task<IActionResult> GetUnreadCount()
    {
        var currentUser = JwtClaimUltils.GetLoginedUser(_httpContextAccessor);
        var currentUserId = JwtClaimUltils.GetUserId(currentUser);
        
        var count = await _chatMessageService.GetUnreadCountAsync(currentUserId);
        return Ok(BaseResponse.OkResponseDto(count));
    }
    
    [HttpPut]
    [Route(WebApiEndpoint.ChatMessage.MarkAsRead)]
    public async Task<IActionResult> MarkAsRead([FromRoute] int id)
    {
        var result = await _chatMessageService.MarkAsReadAsync(id);
        return Ok(BaseResponse.OkResponseDto(result));
    }
    
    [HttpGet]
    [Route(WebApiEndpoint.ChatMessage.GetConversationInfo)]
    public async Task<IActionResult> GetConversationInfo([FromRoute] int otherUserId)
    {
        var currentUser = JwtClaimUltils.GetLoginedUser(_httpContextAccessor);
        var currentUserId = JwtClaimUltils.GetUserId(currentUser);
        
        var conversationInfo = await _chatMessageService.GetConversationInfoAsync(currentUserId, otherUserId);
        return Ok(BaseResponse.OkResponseDto(conversationInfo));
    }
    
    [HttpGet]
    [Route(WebApiEndpoint.ChatMessage.GetConversationList)]
    public async Task<IActionResult> GetConversationList()
    {
        var currentUser = JwtClaimUltils.GetLoginedUser(_httpContextAccessor);
        var currentUserId = JwtClaimUltils.GetUserId(currentUser);
        
        var conversations = await _chatMessageService.GetConversationListAsync(currentUserId);
        return Ok(BaseResponse.OkResponseDto(conversations));
    }
}