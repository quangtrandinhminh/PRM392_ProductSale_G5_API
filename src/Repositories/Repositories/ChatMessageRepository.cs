using Repositories.Base;
using Repositories.Models;

namespace Repositories.Repositories;

public interface IChatMessageRepository : IGenericRepository<ChatMessage>
{
}

public class ChatMessageRepository : GenericRepository<ChatMessage>, IChatMessageRepository
{
    
}