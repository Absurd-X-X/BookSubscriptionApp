using Domain.Entities;

namespace Application.Common.Repositories
{
    public interface INotificationRepository
    {
        Task AddAsync(Notification notification);
        Task<Notification?> GetById(Guid id);
        Task<int> GetUnreadCountAsync(Guid userId);
        Task<ICollection<Notification>> GetAllNotificationtAsync(Guid userId);
        Task<ICollection<Notification>> GetAllUnreadCountAsync(Guid userId);
    }
}
