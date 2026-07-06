using Application.Common.Pagenation;
using Domain.Entities;

namespace Application.Common.Repositories
{
    public interface IBookVersionRepository
    {
        Task AddAsync(BookVersion version);
        Task<BookVersion?> GetCurrentAsync(Guid bookId);
        Task<PagenatedList<BookVersion>> GetByBookIdAsync(Guid bookId, PageRequest request, bool usePaging);
        Task<BookVersion?> GetByIdAsync(Guid id);
        Task SetCurrentAsync(Guid bookId, Guid versionId);
        void Update(BookVersion version);
    }
}