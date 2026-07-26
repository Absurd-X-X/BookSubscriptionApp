using Application.Common.Pagenation;
using Application.Common.Repositories;
using Domain.Entities;
using Domain.Enums;
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
                .Include(c => c.Reviews)
                .Include(c => c.User)
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

        public async Task UpdateReadingGoalAsync(
            Guid readerId,
            ReadingGoalType type,
            int target,
            DateTime? deadline,
            string? motivation)
        {
            var reader = await context.Readers
                .FirstOrDefaultAsync(x => x.Id == readerId);

            if (reader is null) return;

            reader.ReadingGoalType = type;
            reader.ReadingGoalTarget = target;
            reader.ReadingGoalDeadline = deadline;
            reader.ReadingGoalMotivation = motivation;
            reader.DateModified = DateTime.UtcNow;
        }
    }
}
