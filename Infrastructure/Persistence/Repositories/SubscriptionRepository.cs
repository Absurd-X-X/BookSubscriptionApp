using Application.Common.Pagenation;
using Application.Common.Repositories;
using Domain.Entities;
using Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories
{
    public class SubscriptionRepository(AppDbContext context) : ISubscriptionRepository
    {
        public async Task AddAsync(Subscription subscription)

            => await context.Subscriptions.AddAsync(subscription);
        public async Task<Subscription?> GetAsync(Guid id)

            => await context.Subscriptions
                .Include(v => v.Types)
                .Include(x => x.Reader)
                .ThenInclude(x => x.User)
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);

        public Task<Subscription?> GetByReaderIdAsync(Guid readerId, bool isActive)

            => context.Subscriptions
                .Include(v => v.Types)
                .Include(x => x.Reader)
                .ThenInclude(x => x.User)
                .FirstOrDefaultAsync(x => x.ReaderId == readerId &&
                x.IsActive == isActive && !x.IsDeleted);

        public async Task<PagenatedList<Subscription>> GetSubscriptionsAsync(bool usePaging, PageRequest pageRequest)
        {
            var query = context.Subscriptions
                .Include(v => v.Types)
                .Include(x => x.Reader)
                .ThenInclude(x => x.User)
                .AsQueryable();


            if (usePaging)
            {
                var offset = query.Skip((pageRequest.Page - 1) * pageRequest.PageSize).Take(pageRequest.PageSize);

                return new PagenatedList<Subscription>
                {
                    Items = await offset.ToListAsync(),
                    TotalCount = await query.CountAsync(),
                    Page = pageRequest.Page,
                    PageSize = pageRequest.PageSize
                };
            }

            return new PagenatedList<Subscription>
            {
                Items = await query.ToListAsync(),
                TotalCount = await query.CountAsync(),
                Page = pageRequest.Page,
                PageSize = pageRequest.PageSize
            };
        }


        public void Update(Subscription subscription)
        {
            context.Subscriptions.Update(subscription);
        }
    }
}
