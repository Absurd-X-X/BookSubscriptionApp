using Application.Common.Repositories;
using Domain.Entities;
using Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories
{
    public class ReadingProgressRepository(AppDbContext context) : IReadingProgressRepository
    {
        public async Task AddAsync(ReadingProgress readingProgress)
        {
            await context.ReadingProgresses.AddAsync(readingProgress);
        }

        public async Task<ReadingProgress?> GetAsync(Guid ReaderId, Guid BookId)
        {
            return await context.ReadingProgresses
                .Include(rp => rp.Reader)
                .Include(rp => rp.Book)
                .FirstOrDefaultAsync(x => x.ReaderId == ReaderId && x.BookId == BookId  && !x.IsDeleted);
        }

        public async Task<List<ReadingProgress>> GetByLibraryIdAsync(Guid libraryId, DateTime start, DateTime end)
        {
            return await context.ReadingProgresses
                .Where(x => !x.IsDeleted
                    && x.Book.LibraryId == libraryId
                    && x.LastReadAt >= start
                    && x.LastReadAt <= end)
                .Include(x => x.Reader)
                .Include(x => x.Book)
                .ToListAsync();
        }
    }
}
