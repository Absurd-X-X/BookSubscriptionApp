using Application.Common.Pagenation;
using Domain.Entities;

namespace Application.Common.Repositories
{
    public interface IReadingProgressRepository
    {
        Task AddAsync(ReadingProgress readingProgress);

        Task<ReadingProgress?> GetAsync(Guid readerId, Guid bookId);

        Task<List<ReadingProgress>> GetByReaderAsync(Guid readerId);

        Task<List<ReadingProgress>> GetCompletedBooksAsync(Guid readerId);

        Task<List<ReadingProgress>> GetCurrentlyReadingAsync(Guid readerId);

        Task<ReadingProgress?> GetLastReadBookAsync(Guid readerId);

        Task<int> GetCompletedBookCountAsync(Guid readerId);

        Task<int> GetCurrentlyReadingCountAsync(Guid readerId);

        Task<double> GetAverageProgressAsync(Guid readerId);

        Task<int> GetTotalReadingMinutesAsync(Guid readerId);

        Task<List<ReadingProgress>> GetByLibraryIdAsync(Guid libraryId, DateTime start, DateTime end);

        Task<int> GetCompletedBookCountByYearAsync(Guid readerId, int year);

        Task<PagenatedList<ReadingProgress>> GetCurrentlyReadingPagedAsync(
            Guid readerId,
            PageRequest request,
            bool usePaging,
            string? search,
            string? sortBy,
            string? filter);

        Task<int> GetMaxCurrentStreakAsync(Guid readerId);

        // ── New methods for GetMyReadingDashboard ──

        Task<List<ReadingProgress>> GetReadingHistoryAsync(Guid readerId, int take);

        Task<bool[]> GetLastSevenDaysActivityAsync(Guid readerId);

        Task<List<JourneyChartPoint>> GetJourneyChartAsync(Guid readerId, int days);

        Task<MonthlyReadingStats> GetMonthlyStatsAsync(Guid readerId);

        Task<List<GenreMinutes>> GetGenreBreakdownAsync(Guid readerId);
        Task<int> GetTotalPagesReadAsync(Guid readerId);
        Task<int> GetTotalPagesReadByYearAsync(Guid readerId, int year);
        Task<int> GetTotalMinutesReadByYearAsync(Guid readerId, int year);
        Task<ReadingProgress?> GetByIdAsync(Guid id);
        Task SoftDeleteAsync(Guid id);
    }

    public record JourneyChartPoint(DateTime Date, int MinutesRead);

    public record MonthlyReadingStats(int BooksRead, int MinutesRead);

    public record GenreMinutes(string Genre, int MinutesRead);
}