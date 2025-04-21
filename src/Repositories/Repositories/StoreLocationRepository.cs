using Repositories.Base;
using Repositories.Models;

namespace Repositories.Repositories;

public interface IStoreLocationRepository : IGenericRepository<StoreLocation>
{
}

public class StoreLocationRepository : GenericRepository<StoreLocation>, IStoreLocationRepository
{
    
}