using Application.Common.Repositories;
using Domain.Entities;
using Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories
{
    public class NotificationRepository(AppDbContext context) : INotificationRepository
    {
        public async Task AddAsync(Notification notification)

            => await context.Notifications.AddAsync(notification);

        public async Task<ICollection<Notification>> GetAllNotificationtAsync(Guid userId)

            => await context.Notifications
                .Where(x => x.UserId == userId)
                .ToListAsync();

        public async Task<ICollection<Notification>> GetAllUnreadCountAsync(Guid userId)

            => await context.Notifications
                .Where(x => x.UserId == userId && !x.IsDeleted)
                .ToListAsync();

        public async Task<Notification?> GetById(Guid id)

            => await context.Notifications
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);

        public async Task<int> GetUnreadCountAsync(Guid userId)

            => await context.Notifications
                .CountAsync(x => x.UserId == userId
                && !x.IsRead &&
                !x.IsDeleted);
    }
}
