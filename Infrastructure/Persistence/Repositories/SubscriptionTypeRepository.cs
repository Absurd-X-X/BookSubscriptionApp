using Application.Common.Repositories;
using Domain.Entities;
using Domain.Enums;
using Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories
{
    public class SubscriptionTypeRepository(AppDbContext context) : ISubscriptionTypeRepository
    {
        public async Task AddAsync(SubscriptionType subscriptionType)
        
           => await context.SubscriptionTypes.AddAsync(subscriptionType);
        

        public async Task<ICollection<SubscriptionType>> GetAllAsync()
        {
            return await context.SubscriptionTypes.Where(x => !x.IsDeleted).ToListAsync();
        }

        public async Task<ICollection<SubscriptionType>> GetByCycleAsync(BillingCycle cycle)
        {
            return await context.SubscriptionTypes.
                Where(x => x.Cycle == cycle && !x.IsDeleted).ToListAsync();
        }

        public async Task<SubscriptionType?> GetByIdAsync(Guid id)
        {
            return await context.SubscriptionTypes
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);
        }

        public async Task<SubscriptionType?> IsExistAsync(string typeName, BillingCycle cycle)
        {
            return await context.SubscriptionTypes
                .FirstOrDefaultAsync(x => x.TypeName == typeName && x.Cycle == cycle && !x.IsDeleted);
        }
    }
}
