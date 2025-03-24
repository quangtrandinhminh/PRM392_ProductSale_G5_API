using Microsoft.EntityFrameworkCore;
using Repositories.Base;
using Repositories.Models;

namespace Repositories.Repositories;

public interface ICartRepository : IGenericRepository<Cart>
{
    Task<Cart?> GetCartByUserIdAsync(int userId, string status);
}

public class CartRepository : GenericRepository<Cart>, ICartRepository
{
    public async Task<Cart?> GetCartByUserIdAsync(int userId, string status)
    {
        return await DbSet
            .Include(x => x.CartItems)
            .ThenInclude(x => x.Product)
            .FirstOrDefaultAsync(x => x.UserId == userId && x.Status == status);
    }
}