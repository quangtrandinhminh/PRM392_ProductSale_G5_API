using Repositories.Base;
using Repositories.Models;

namespace Repositories.Repositories;

public interface IProductRepository : IGenericRepository<Product>
{
}

public class ProductRepository : GenericRepository<Product>, IProductRepository
{
    
}