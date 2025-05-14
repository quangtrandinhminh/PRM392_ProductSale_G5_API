using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Service.Utils;
using Services.ApiModels;
using Services.ApiModels.UserDevice;
using Services.Constants;
using Services.Services;
using Serilog;
using ILogger = Serilog.ILogger;

namespace PRM_ProductSale_G5.Controllers;

[ApiController]
[Authorize]
public class UserDeviceController : ControllerBase
{
    private readonly IUserDeviceService _userDeviceService;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger _logger;
    
    public UserDeviceController(IServiceProvider serviceProvider)
    {
        _userDeviceService = serviceProvider.GetRequiredService<IUserDeviceService>();
        _httpContextAccessor = serviceProvider.GetRequiredService<IHttpContextAccessor>();
        _logger = serviceProvider.GetRequiredService<ILogger>();
    }
    
    [HttpPost]
    [Route(WebApiEndpoint.UserDevice.RegisterDevice)]
    public async Task<IActionResult> RegisterDevice([FromBody] UserDeviceRegisterRequest request)
    {
        var currentUser = JwtClaimUltils.GetLoginedUser(_httpContextAccessor);
        var currentUserId = JwtClaimUltils.GetUserId(currentUser);
        
        _logger.Information("Đang đăng ký thiết bị mới cho người dùng {UserId}. DeviceType: {DeviceType}", 
                         currentUserId, request.DeviceType);
        
        try
        {
            var device = await _userDeviceService.RegisterDeviceAsync(currentUserId, request);
            
            _logger.Information(
                "Đã đăng ký thiết bị thành công. UserDeviceId: {UserDeviceId}, UserId: {UserId}, DeviceType: {DeviceType}", 
                device.UserDeviceId, device.UserId, device.DeviceType);
            
            return Ok(BaseResponse.OkResponseDto(device));
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Lỗi khi đăng ký thiết bị cho người dùng {UserId}: {ErrorMessage}", 
                       currentUserId, ex.Message);
            throw;
        }
    }
    
    [HttpDelete]
    [Route(WebApiEndpoint.UserDevice.UnregisterDevice)]
    public async Task<IActionResult> UnregisterDevice([FromQuery] string deviceToken)
    {
        var currentUser = JwtClaimUltils.GetLoginedUser(_httpContextAccessor);
        var currentUserId = JwtClaimUltils.GetUserId(currentUser);
        
        _logger.Information("Đang hủy đăng ký thiết bị cho người dùng {UserId}. Token: {TokenPreview}...", 
                         currentUserId, deviceToken.Substring(0, Math.Min(10, deviceToken.Length)));
        
        try
        {
            var result = await _userDeviceService.UnregisterDeviceAsync(currentUserId, deviceToken);
            
            if (result)
            {
                _logger.Information("Đã hủy đăng ký thiết bị thành công cho người dùng {UserId}", currentUserId);
            }
            else
            {
                _logger.Warning("Không tìm thấy thiết bị để hủy đăng ký cho người dùng {UserId}", currentUserId);
            }
            
            return Ok(BaseResponse.OkResponseDto(result));
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Lỗi khi hủy đăng ký thiết bị cho người dùng {UserId}: {ErrorMessage}", 
                       currentUserId, ex.Message);
            throw;
        }
    }
    
    [HttpGet]
    [Route(WebApiEndpoint.UserDevice.GetUserDevices)]
    public async Task<IActionResult> GetUserDevices()
    {
        var currentUser = JwtClaimUltils.GetLoginedUser(_httpContextAccessor);
        var currentUserId = JwtClaimUltils.GetUserId(currentUser);
        
        _logger.Information("Đang lấy danh sách thiết bị của người dùng {UserId}", currentUserId);
        
        try
        {
            var devices = await _userDeviceService.GetUserDevicesAsync(currentUserId);
            var deviceList = devices.ToList();
            
            _logger.Information("Đã lấy {DeviceCount} thiết bị của người dùng {UserId}", 
                            deviceList.Count, currentUserId);
            
            return Ok(BaseResponse.OkResponseDto(devices));
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Lỗi khi lấy danh sách thiết bị của người dùng {UserId}: {ErrorMessage}", 
                       currentUserId, ex.Message);
            throw;
        }
    }
}
