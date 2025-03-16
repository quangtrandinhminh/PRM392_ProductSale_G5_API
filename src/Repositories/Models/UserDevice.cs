using System.ComponentModel.DataAnnotations;

namespace Repositories.Models;

public partial class UserDevice
{
    [Key]
    public int UserDeviceId { get; set; }
    
    public int? UserId { get; set; }
    
    public string DeviceToken { get; set; }
    
    public string DeviceType { get; set; }
    
    public DateTime LastUsed { get; set; }
    
    public virtual User User { get; set; }
}
