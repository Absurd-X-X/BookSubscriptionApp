// Application.Common.Repositories/INotificationRepository.cs
using Domain.Entities;

namespace Application.Common.Repositories
{
    public interface INotificationRepository
    {
        Task AddAsync(Notification notification);
        Task<ICollection<Notification>> GetAllNotificationtAsync(Guid userId);
        Task<ICollection<Notification>> GetAllUnreadCountAsync(Guid userId);
        Task<Notification?> GetById(Guid id);
        Task<int> GetUnreadCountAsync(Guid userId);
        Task MarkAsReadAsync(Guid id);
        Task MarkAsUnreadAsync(Guid id);
        Task SoftDeleteAsync(Guid id);
        Task ArchiveAsync(Guid id);
        Task MarkAllAsReadAsync(Guid userId);
    }
}