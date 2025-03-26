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
    
    public ChatMessageController(IServiceProvider serviceProvider)
    {
        _chatMessageService = serviceProvider.GetRequiredService<IChatMessageService>();
        _chatHubContext = serviceProvider.GetRequiredService<IHubContext<ChatHub>>();
        _httpContextAccessor = serviceProvider.GetRequiredService<IHttpContextAccessor>();
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
        
        // Đảm bảo người dùng hiện tại là người gửi tin nhắn
        request.UserId = currentUserId;
        
        var message = await _chatMessageService.CreateChatMessageAsync(request);
        
        // Gửi tin nhắn qua SignalR
        await _chatHubContext.Clients.All.SendAsync("ReceiveMessage", message);
        
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
}