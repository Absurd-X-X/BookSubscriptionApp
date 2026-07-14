using Domain.Entities;

namespace Application.Common.Repositories
{
    public interface IReadingListRepository
    {
        Task AddAsync(ReadingListItem item);
        Task RemoveAsync(ReadingListItem item);
        Task<ReadingListItem?> GetAsync(Guid readerId, Guid bookId);
        Task<bool> IsInReadingListAsync(Guid readerId, Guid bookId);
        Task<List<Book>> GetReaderReadingListAsync(Guid readerId);
    }
}