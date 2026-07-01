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
    }
}
