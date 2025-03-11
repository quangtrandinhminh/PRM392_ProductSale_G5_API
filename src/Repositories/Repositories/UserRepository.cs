using Repositories.Base;
using Repositories.Models;

namespace Repositories.Repositories;

public interface IUserRepository : IGenericRepository<User>
{
}

public class UserRepository : GenericRepository<User>, IUserRepository
{
    public UserRepository()
    {
    }
}