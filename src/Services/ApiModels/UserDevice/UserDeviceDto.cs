using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace Services.ApiModels.UserDevice;

public class UserDeviceDto
{
    public int UserDeviceId { get; set; }
    public int? UserId { get; set; }
    public string DeviceToken { get; set; }
    public string DeviceType { get; set; }
    public DateTime LastUsed { get; set; }
}

public class UserDeviceListDto
{
    public List<UserDeviceDto> Devices { get; set; } = new List<UserDeviceDto>();
    public int Count => Devices.Count;
}

public class UserDeviceRegisterRequest
{
    [Required]
    public string DeviceToken { get; set; }
    
    [Required]
    public string DeviceType { get; set; }
}