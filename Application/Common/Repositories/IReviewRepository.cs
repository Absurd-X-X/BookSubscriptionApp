using Application.Common.Pagenation;
using Domain.Entities;

namespace Application.Common.Repositories
{
    public interface IReviewRepository
    {
        Task AddAsync(Review review);
        Task<PagenatedList<Review>> GetAllAsync(PageRequest request, bool usePaging);
        Task<PagenatedList<Review>> GetByBookIdAsync(PageRequest request, bool usePaging, Guid bookId);
        Task<PagenatedList<Review>> GetByLibraryIdAsync(PageRequest request, bool usePaging, Guid libraryId);
        Task<Review?> GetByIdAsync(Guid reviewId);
        Task<int> CountByBookIdAsync(Guid bookId);
        Task<double> GetAverageRatingForBookAsync(Guid bookId);
        Task<int> CountByReaderIdAsync(Guid readerId);
        Task<List<Review>> GetByLibraryIdAndDateRangeAsync(Guid libraryId, DateTime start, DateTime end);

    }
}
