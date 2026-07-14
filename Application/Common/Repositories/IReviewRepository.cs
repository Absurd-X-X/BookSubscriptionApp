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
        Task<List<Review>> GetByReaderIdAsync(Guid readerId, int take);
        Task<double> GetAverageRatingGivenByReaderAsync(Guid readerId);
        Task<Dictionary<int, int>> GetRatingDistributionByReaderAsync(Guid readerId);
        Task<double> GetAverageRatingGivenByReaderInYearAsync(Guid readerId, int year);
        Task<PagenatedList<Review>> GetPagedByReaderIdAsync(
            Guid readerId, PageRequest request, bool usePaging,
            string? search = null, string? sortBy = null,
            int? ratingFilter = null, Guid? bookIdFilter = null);

        Task<List<(Guid BookId, string Title)>> GetReviewedBookOptionsAsync(Guid readerId);

    }
}
