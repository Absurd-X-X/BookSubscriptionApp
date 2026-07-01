using Domain.Entities;

namespace Application.Common.Repositories
{
    public interface IReadingProgressRepository
    {
        Task AddAsync(ReadingProgress readingProgress);
        Task<ReadingProgress?> GetAsync(Guid ReaderId, Guid BookId); 
    }
}
