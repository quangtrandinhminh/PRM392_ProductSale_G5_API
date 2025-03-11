using Repositories.Base;
using Repositories.Models;

namespace Repositories.Repositories;

public interface ICartRepository : IGenericRepository<Cart>
{
}

public class CartRepository : GenericRepository<Cart>, ICartRepository
{
    
}