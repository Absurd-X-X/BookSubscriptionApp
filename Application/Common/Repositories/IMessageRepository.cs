using Domain.Entities;

namespace Application.Repositories
{
    public interface IMessageRepository
    {
        Task AddAsync(Message message);
        Task<Message?> GetByIdAsync(Guid id);
    }
}