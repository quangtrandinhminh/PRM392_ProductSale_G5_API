using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Repositories.Models;
using Repositories.Repositories;
using Serilog;
using Service.Utils;
using Services.ApiModels.UserDevice;
using Services.Constants;
using Services.Exceptions;
using Services.Mapper;

namespace Services.Services;

public interface IUserDeviceService
{
    Task<UserDeviceDto> RegisterDeviceAsync(int userId, UserDeviceRegisterRequest request);
    Task<bool> UnregisterDeviceAsync(int userId, string deviceToken);
    Task<IEnumerable<UserDeviceDto>> GetUserDevicesAsync(int userId);
}

public class UserDeviceService : IUserDeviceService
{
    private readonly IUserDeviceRepository _userDeviceRepository;
    private readonly ILogger _logger;
    private readonly MapperlyMapper _mapper;
    
    public UserDeviceService(IServiceProvider serviceProvider)
    {
        _userDeviceRepository = serviceProvider.GetRequiredService<IUserDeviceRepository>();
        _logger = serviceProvider.GetRequiredService<ILogger>();
        _mapper = serviceProvider.GetRequiredService<MapperlyMapper>();
    }
    
    public async Task<UserDeviceDto> RegisterDeviceAsync(int userId, UserDeviceRegisterRequest request)
    {
        _logger.Information("Đăng ký thiết bị cho người dùng {UserId}", userId);
        
        var existingDevice = await _userDeviceRepository.GetByTokenAsync(request.DeviceToken);
        
        if (existingDevice != null)
        {
            // Nếu token đã tồn tại nhưng thuộc người dùng khác, cập nhật lại
            if (existingDevice.UserId != userId)
            {
                existingDevice.UserId = userId;
            }
            
            existingDevice.LastUsed = DateTime.UtcNow;
            _userDeviceRepository.Update(existingDevice);
            await _userDeviceRepository.SaveChangeAsync();
            
            return _mapper.Map(existingDevice);
        }
        
        // Tạo mới nếu chưa tồn tại
        var userDevice = new UserDevice
        {
            UserId = userId,
            DeviceToken = request.DeviceToken,
            DeviceType = request.DeviceType,
            LastUsed = DateTime.UtcNow
        };
        
        await _userDeviceRepository.AddAsync(userDevice);
        await _userDeviceRepository.SaveChangeAsync();
        
        return _mapper.Map(userDevice);
    }
    
    public async Task<bool> UnregisterDeviceAsync(int userId, string deviceToken)
    {
        _logger.Information("Hủy đăng ký thiết bị cho người dùng {UserId}", userId);
        return await _userDeviceRepository.RemoveDeviceAsync(userId, deviceToken);
    }
    
    public async Task<IEnumerable<UserDeviceDto>> GetUserDevicesAsync(int userId)
    {
        _logger.Information("Lấy danh sách thiết bị của người dùng {UserId}", userId);
        var devices = await _userDeviceRepository.GetUserDevicesAsync(userId);
        return _mapper.Map(devices);
    }
}