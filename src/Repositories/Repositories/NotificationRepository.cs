using Repositories.Base;
using Repositories.Models;

namespace Repositories.Repositories;

public interface INotificationRepository : IGenericRepository<Notification>
{
}

public class NotificationRepository : GenericRepository<Notification>, INotificationRepository
{
    
}