using Repositories.Base;
using Repositories.Models;

namespace Repositories.Repositories;

public interface IOrderRepository : IGenericRepository<Order>
{
}

public class OrderRepository : GenericRepository<Order>, IOrderRepository
{
    
}