using Application.Common.Repositories;
using Domain.Entities;
using Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace Persistence.Repositories
{
    public class ReadingListRepository(AppDbContext context)
        : IReadingListRepository
    {
        public async Task AddAsync(ReadingListItem item)
        {
            await context.ReadingListItems.AddAsync(item);
        }

        public async Task RemoveAsync(ReadingListItem item)
        {
            context.ReadingListItems.Remove(item);
            await Task.CompletedTask;
        }

        public async Task<ReadingListItem?> GetAsync(Guid readerId, Guid bookId)
        {
            return await context.ReadingListItems
                .FirstOrDefaultAsync(x =>
                    x.ReaderId == readerId &&
                    x.BookId == bookId);
        }

        public async Task<bool> IsInReadingListAsync(Guid readerId, Guid bookId)
        {
            return await context.ReadingListItems
                .AnyAsync(x =>
                    x.ReaderId == readerId &&
                    x.BookId == bookId);
        }

        public async Task<List<Book>> GetReaderReadingListAsync(Guid readerId)
        {
            return await context.ReadingListItems
                .Where(x => x.ReaderId == readerId)
                .Select(x => x.Book)
                .ToListAsync();
        }
    }
}