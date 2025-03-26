using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Repositories.Models;

public partial class UserDevice
{
    public int UserDeviceId { get; set; }

    public int? UserId { get; set; }

    public string DeviceToken { get; set; } = null!;

    public string DeviceType { get; set; } = null!;

    public DateTime LastUsed { get; set; }

    public virtual User? User { get; set; }
}
