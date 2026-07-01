using Domain.Entities;
using Domain.Enums;

namespace Application.Common.Repositories
{
    public interface ISubscriptionTypeRepository
    {
        Task AddAsync(SubscriptionType subscriptionType);
        Task<SubscriptionType?> GetByIdAsync(Guid id);
        Task<ICollection<SubscriptionType>> GetByCycleAsync(BillingCycle cycle);
        Task<ICollection<SubscriptionType>> GetAllAsync();
    }
}
