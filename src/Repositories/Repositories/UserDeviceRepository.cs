using Microsoft.EntityFrameworkCore;
using Repositories.Base;
using Repositories.Models;

namespace Repositories.Repositories;

public interface IUserDeviceRepository : IGenericRepository<UserDevice>
{
    Task<IEnumerable<UserDevice>> GetUserDevicesAsync(int userId);
    Task<UserDevice> GetByTokenAsync(string token);
    Task<bool> RemoveDeviceAsync(int userId, string token);
}

public class UserDeviceRepository : GenericRepository<UserDevice>, IUserDeviceRepository
{
    private readonly AppDbContext _context;
    
    public UserDeviceRepository(AppDbContext context) : base(context)
    {
        _context = context;
    }
    
    public async Task<IEnumerable<UserDevice>> GetUserDevicesAsync(int userId)
    {
        return await _context.Set<UserDevice>()
            .Where(d => d.UserId == userId)
            .ToListAsync();
    }
    
    public async Task<UserDevice> GetByTokenAsync(string token)
    {
        return await _context.Set<UserDevice>()
            .FirstOrDefaultAsync(d => d.DeviceToken == token);
    }
    
    public async Task<bool> RemoveDeviceAsync(int userId, string token)
    {
        var device = await _context.Set<UserDevice>()
            .FirstOrDefaultAsync(d => d.UserId == userId && d.DeviceToken == token);
            
        if (device == null) return false;
        
        _context.Set<UserDevice>().Remove(device);
        await _context.SaveChangesAsync();
        return true;
    }
}
