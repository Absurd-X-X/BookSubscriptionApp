using Domain.Entities;

namespace Application.Repositories
{
    public interface IConversationRepository
    {
        Task AddAsync(Conversation conversation);
        Task<Conversation?> GetByIdAsync(Guid id);
        Task<Conversation?> GetGroupAsync(Guid adminId);
        Task<ICollection<Conversation>> GetByUserIdAsync(Guid userId);
        Task<ICollection<Conversation>> GetAllAsync();
        Task<Conversation?> GetPrivateConversationAsync(Guid senderId, Guid userId);
    }
}