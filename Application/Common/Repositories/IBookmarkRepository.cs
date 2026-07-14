using Application.Common.Pagenation;
using Domain.Entities;

namespace Application.Common.Repositories
{
    public interface IBookmarkRepository
    {
        Task AddAsync(Bookmark bookmark);

        Task RemoveAsync(Bookmark bookmark);

        Task<Bookmark?> GetByIdAsync(Guid id);

        Task<List<Bookmark>> GetReaderBookmarksAsync(Guid readerId);

        Task<PagenatedList<Bookmark>> GetReaderBookmarksPagedAsync(
            Guid readerId,
            PageRequest page,
            string? search,
            string? sortBy);
    }
}