using Application.Common.Pagenation;
using Domain.Entities;

namespace Application.Common.Repositories
{
    public interface IFavoriteRepository
    {
        Task AddAsync(Favorite favorite);

        Task RemoveAsync(Favorite favorite);

        Task<Favorite?> GetAsync(Guid readerId, Guid bookId);

        Task<bool> IsFavoriteAsync(Guid readerId, Guid bookId);

        Task<List<Book>> GetReaderFavoritesAsync(Guid readerId);

        Task<int> GetBookFavoriteCountAsync(Guid bookId);

        Task<PagenatedList<Favorite>> GetReaderFavoritesPagedAsync(
            Guid readerId,
            PageRequest page,
            string? search,
            Guid? categoryId,
            string? sortBy);
    }
}