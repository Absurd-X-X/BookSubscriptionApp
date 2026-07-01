using Application.Common.Pagenation;
using Domain.Entities;

namespace Application.Common.Repositories
{
    public interface ISubscriptionRepository
    {
        Task AddAsync(Subscription subscription);
        Task<Subscription?> GetAsync(Guid id);
        Task<Subscription?> GetByReaderIdAsync(Guid readerId, bool isActive);
        Task<PagenatedList<Subscription>> GetSubscriptionsAsync(bool usePaging, PageRequest pageRequest);
        void Update(Subscription subscription);
    }
}
