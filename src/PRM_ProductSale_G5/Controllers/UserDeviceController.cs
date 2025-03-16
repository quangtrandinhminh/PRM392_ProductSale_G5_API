using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Service.Utils;
using Services.ApiModels;
using Services.ApiModels.UserDevice;
using Services.Constants;
using Services.Services;

namespace PRM_ProductSale_G5.Controllers;

[ApiController]
[Authorize]
public class UserDeviceController : ControllerBase
{
    private readonly IUserDeviceService _userDeviceService;
    private readonly IHttpContextAccessor _httpContextAccessor;
    
    public UserDeviceController(IServiceProvider serviceProvider)
    {
        _userDeviceService = serviceProvider.GetRequiredService<IUserDeviceService>();
        _httpContextAccessor = serviceProvider.GetRequiredService<IHttpContextAccessor>();
    }
    
    [HttpPost]
    [Route(WebApiEndpoint.UserDevice.RegisterDevice)]
    public async Task<IActionResult> RegisterDevice([FromBody] UserDeviceRegisterRequest request)
    {
        var currentUser = JwtClaimUltils.GetLoginedUser(_httpContextAccessor);
        var currentUserId = JwtClaimUltils.GetUserId(currentUser);
        
        var device = await _userDeviceService.RegisterDeviceAsync(currentUserId, request);
        return Ok(BaseResponse.OkResponseDto(device));
    }
    
    [HttpDelete]
    [Route(WebApiEndpoint.UserDevice.UnregisterDevice)]
    public async Task<IActionResult> UnregisterDevice([FromQuery] string deviceToken)
    {
        var currentUser = JwtClaimUltils.GetLoginedUser(_httpContextAccessor);
        var currentUserId = JwtClaimUltils.GetUserId(currentUser);
        
        var result = await _userDeviceService.UnregisterDeviceAsync(currentUserId, deviceToken);
        return Ok(BaseResponse.OkResponseDto(result));
    }
    
    [HttpGet]
    [Route(WebApiEndpoint.UserDevice.GetUserDevices)]
    public async Task<IActionResult> GetUserDevices()
    {
        var currentUser = JwtClaimUltils.GetLoginedUser(_httpContextAccessor);
        var currentUserId = JwtClaimUltils.GetUserId(currentUser);
        
        var devices = await _userDeviceService.GetUserDevicesAsync(currentUserId);
        return Ok(BaseResponse.OkResponseDto(devices));
    }
}
