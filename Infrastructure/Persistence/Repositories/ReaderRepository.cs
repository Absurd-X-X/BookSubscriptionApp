using Application.Common.Pagenation;
using Application.Common.Repositories;
using Domain.Entities;
using Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories
{
    public class ReaderRepository(AppDbContext context) : IReaderRepository
    {
        public async Task AddAsync(Reader reader)

            => await context.Readers.AddAsync(reader);

        public async Task<Reader?> GetByIdAsync(Guid id)

            => await context.Readers
                .Include(r => r.Subscriptions)
                .FirstOrDefaultAsync(r => r.Id == id);

        public Task<Reader?> GetByEmailAsync(string email)

            => context.Readers
                .Include(r => r.Subscriptions
                .Where(s => s.IsActive))
                .FirstOrDefaultAsync(r => r.Email == email);

        public async Task<PagenatedList<Reader>> GetReadersAsync(PageRequest request, bool usePaging)
        {
            var query = context.Readers.AsQueryable();

            int totalCount = await query.CountAsync();

            if (usePaging)
            {
                var set = query.Skip((request.Page - 1) * request.PageSize).Take(request.PageSize);

                return new PagenatedList<Reader>
                {
                    Items = set.Include(x => x.Subscriptions),
                    Page = request.Page,
                    PageSize = request.PageSize,
                    TotalCount = totalCount
                };
            }

            return new PagenatedList<Reader>
            {
                Items = query,
                TotalCount = totalCount
            };
        }
    }
}
