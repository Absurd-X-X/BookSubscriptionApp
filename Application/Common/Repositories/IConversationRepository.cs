using Domain.Entities;

namespace Application.Repositories
{
    public interface IConversationRepository
    {
        Task AddAsync(Conversation conversation);
        Task<Conversation?> GetByIdAsync(Guid id);
        Task<ICollection<Conversation>> GetByUserIdAsync(Guid userId);
        Task<ICollection<Conversation>> GetAllAsync();
    }
}