using Repositories.Base;
using Repositories.Models;

namespace Repositories.Repositories;

public interface ICartItemRepository : IGenericRepository<CartItem>
{
}

public class CartItemRepository : GenericRepository<CartItem>, ICartItemRepository
{

}