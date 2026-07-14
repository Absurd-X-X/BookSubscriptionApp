using Application.Common.Pagenation;
using Application.Common.Repositories;
using Domain.Entities;
using Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace Persistence.Repositories
{
    public class ReadingProgressRepository(AppDbContext context)
        : IReadingProgressRepository
    {
        public async Task AddAsync(ReadingProgress readingProgress)
        {
            await context.ReadingProgresses.AddAsync(readingProgress);
        }

        public async Task<ReadingProgress?> GetAsync(Guid readerId, Guid bookId)
        {
            return await context.ReadingProgresses
                .Include(x => x.Book)
                .FirstOrDefaultAsync(x =>
                    x.ReaderId == readerId &&
                    x.BookId == bookId);
        }

        public async Task<List<ReadingProgress>> GetByReaderAsync(Guid readerId)
        {
            return await context.ReadingProgresses
                .Include(x => x.Book)
                .Where(x => x.ReaderId == readerId)
                .OrderByDescending(x => x.LastReadDate)
                .ToListAsync();
        }

        public async Task<List<ReadingProgress>> GetCompletedBooksAsync(Guid readerId)
        {
            return await context.ReadingProgresses
                .Include(x => x.Book)
                .Where(x => x.ReaderId == readerId &&
                            x.IsCompleted)
                .OrderByDescending(x => x.LastReadDate)
                .ToListAsync();
        }

        public async Task<List<ReadingProgress>> GetCurrentlyReadingAsync(Guid readerId)
        {
            return await context.ReadingProgresses
                .Include(x => x.Book)
                .Where(x => x.ReaderId == readerId &&
                            !x.IsCompleted)
                .OrderByDescending(x => x.LastReadDate)
                .ToListAsync();
        }

        public async Task<ReadingProgress?> GetLastReadBookAsync(Guid readerId)
        {
            return await context.ReadingProgresses
                .Include(x => x.Book)
                .Where(x => x.ReaderId == readerId)
                .OrderByDescending(x => x.LastReadDate)
                .FirstOrDefaultAsync();
        }

        public async Task<int> GetCompletedBookCountAsync(Guid readerId)
        {
            return await context.ReadingProgresses
                .CountAsync(x =>
                    x.ReaderId == readerId &&
                    x.IsCompleted);
        }

        public async Task<int> GetCurrentlyReadingCountAsync(Guid readerId)
        {
            return await context.ReadingProgresses
                .CountAsync(x =>
                    x.ReaderId == readerId &&
                    !x.IsCompleted);
        }

        public async Task<double> GetAverageProgressAsync(Guid readerId)
        {
            var progresses = await context.ReadingProgresses
                .Where(x => x.ReaderId == readerId)
                .Select(x => x.ProgressPercentage)
                .ToListAsync();

            if (!progresses.Any())
                return 0;

            return progresses.Average();
        }

        public async Task<int> GetTotalReadingMinutesAsync(Guid readerId)
        {
            return await context.ReadingProgresses
                .Where(x => x.ReaderId == readerId)
                .SumAsync(x => x.TotalMinutesRead);
        }

        public async Task<List<ReadingProgress>> GetByLibraryIdAsync(
            Guid libraryId,
            DateTime start,
            DateTime end)
        {
            return await context.ReadingProgresses
                .Include(x => x.Reader)
                .Include(x => x.Book)
                .Where(x =>
                    x.Book.LibraryId == libraryId &&
                    x.LastReadDate >= start &&
                    x.LastReadDate <= end)
                .OrderByDescending(x => x.LastReadDate)
                .ToListAsync();
        }

        public async Task<int> GetCompletedBookCountByYearAsync(Guid readerId, int year)
        {
            return await context.ReadingProgresses
                .CountAsync(x =>
                    x.ReaderId == readerId &&
                    x.IsCompleted &&
                    x.LastReadDate.HasValue &&
                    x.LastReadDate.Value.Year == year);
        }

        public async Task<PagenatedList<ReadingProgress>> GetCurrentlyReadingPagedAsync(
            Guid readerId,
            PageRequest request,
            bool usePaging,
            string? search,
            string? sortBy,
            string? filter)
        {
            var query = context.ReadingProgresses
                .Include(x => x.Book)
                .Where(x => x.ReaderId == readerId &&
                            !x.IsCompleted &&
                            !x.IsDeleted)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var s = search.Trim();
                query = query.Where(x =>
                    x.Book.Title.Contains(s) ||
                    x.Book.Author.Contains(s));
            }

            query = filter switch
            {
                "under50" => query.Where(x => x.ProgressPercentage < 50),
                "over50" => query.Where(x => x.ProgressPercentage >= 50),
                "over75" => query.Where(x => x.ProgressPercentage >= 75),
                "recent" => query.Where(x => x.LastReadDate.HasValue &&
                                              x.LastReadDate.Value >= DateTime.UtcNow.AddDays(-3)),
                _ => query
            };

            query = sortBy switch
            {
                "title-asc" => query.OrderBy(x => x.Book.Title),
                "title-desc" => query.OrderByDescending(x => x.Book.Title),
                "progress-asc" => query.OrderBy(x => x.ProgressPercentage),
                "progress-desc" => query.OrderByDescending(x => x.ProgressPercentage),
                "date-added" => query.OrderByDescending(x => x.DateCreated),
                _ => query.OrderByDescending(x => x.LastReadDate)
            };

            var totalCount = await query.CountAsync();

            if (usePaging)
            {
                int currentPage = request.Page < 1 ? 1 : request.Page;
                int pageSize = request.PageSize < 1 ? 5 : request.PageSize;

                var items = await query
                    .Skip((currentPage - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                return new PagenatedList<ReadingProgress>
                {
                    Items = items,
                    Page = currentPage,
                    PageSize = pageSize,
                    TotalCount = totalCount
                };
            }

            return new PagenatedList<ReadingProgress>
            {
                Items = await query.ToListAsync(),
                TotalCount = totalCount
            };
        }

        public async Task<int> GetMaxCurrentStreakAsync(Guid readerId)
        {
            var streaks = await context.ReadingProgresses
                .Where(x => x.ReaderId == readerId && !x.IsDeleted)
                .Select(x => x.ReadingStreak)
                .ToListAsync();

            return streaks.Any() ? streaks.Max() : 0;
        }

        // ── New methods for GetMyReadingDashboard ──

        public async Task<List<ReadingProgress>> GetReadingHistoryAsync(Guid readerId, int take)
        {
            return await context.ReadingProgresses
                .Include(x => x.Book)
                .Where(x => x.ReaderId == readerId && !x.IsDeleted)
                .OrderByDescending(x => x.LastReadDate)
                .Take(take)
                .ToListAsync();
        }

        public async Task<bool[]> GetLastSevenDaysActivityAsync(Guid readerId)
        {
            var start = DateTime.UtcNow.Date.AddDays(-6);

            var activeDates = await context.ReadingProgresses
                .Where(x => x.ReaderId == readerId &&
                            !x.IsDeleted &&
                            x.LastReadDate.HasValue &&
                            x.LastReadDate.Value.Date >= start)
                .Select(x => x.LastReadDate!.Value.Date)
                .Distinct()
                .ToListAsync();

            return Enumerable.Range(0, 7)
                .Select(i => activeDates.Contains(start.AddDays(i)))
                .ToArray();
        }

        public async Task<List<JourneyChartPoint>> GetJourneyChartAsync(Guid readerId, int days)
        {
            var start = DateTime.UtcNow.Date.AddDays(-(days - 1));

            // NOTE: TotalMinutesRead is a running total per ReadingProgress row,
            // not a per-day log, so this buckets by LastReadDate only.
            // For an accurate day-by-day minutes chart, a ReadingSession log
            // table (readerId, bookId, date, minutes) would be needed.
            return await context.ReadingProgresses
                .Where(x => x.ReaderId == readerId &&
                            !x.IsDeleted &&
                            x.LastReadDate.HasValue &&
                            x.LastReadDate.Value.Date >= start)
                .GroupBy(x => x.LastReadDate!.Value.Date)
                .Select(g => new JourneyChartPoint(g.Key, g.Sum(x => x.TotalMinutesRead)))
                .ToListAsync();
        }

        public async Task<MonthlyReadingStats> GetMonthlyStatsAsync(Guid readerId)
        {
            var now = DateTime.UtcNow;

            var booksThisMonth = await context.ReadingProgresses
                .CountAsync(x => x.ReaderId == readerId &&
                                  !x.IsDeleted &&
                                  x.IsCompleted &&
                                  x.LastReadDate.HasValue &&
                                  x.LastReadDate.Value.Year == now.Year &&
                                  x.LastReadDate.Value.Month == now.Month);

            var minutesThisMonth = await context.ReadingProgresses
                .Where(x => x.ReaderId == readerId &&
                            !x.IsDeleted &&
                            x.LastReadDate.HasValue &&
                            x.LastReadDate.Value.Year == now.Year &&
                            x.LastReadDate.Value.Month == now.Month)
                .SumAsync(x => x.TotalMinutesRead);

            return new MonthlyReadingStats(booksThisMonth, minutesThisMonth);
        }

        public async Task<List<GenreMinutes>> GetGenreBreakdownAsync(Guid readerId)
        {
            return await context.ReadingProgresses
                .Include(x => x.Book)
                .Where(x => x.ReaderId == readerId && !x.IsDeleted)
                .GroupBy(x => x.Book.Genre)
                .Select(g => new GenreMinutes(g.Key, g.Sum(x => x.TotalMinutesRead)))
                .ToListAsync();
        }

        public async Task<int> GetTotalPagesReadAsync(Guid readerId)
        {
            return await context.ReadingProgresses
                .Where(x => x.ReaderId == readerId && !x.IsDeleted)
                .SumAsync(x => x.TotalPagesRead);
        }

        public async Task<int> GetTotalPagesReadByYearAsync(Guid readerId, int year)
        {
            return await context.ReadingProgresses
                .Where(x => x.ReaderId == readerId &&
                            !x.IsDeleted &&
                            x.LastReadDate.HasValue &&
                            x.LastReadDate.Value.Year == year)
                .SumAsync(x => x.TotalPagesRead);
        }

        public async Task<int> GetTotalMinutesReadByYearAsync(Guid readerId, int year)
        {
            return await context.ReadingProgresses
                .Where(x => x.ReaderId == readerId &&
                            !x.IsDeleted &&
                            x.LastReadDate.HasValue &&
                            x.LastReadDate.Value.Year == year)
                .SumAsync(x => x.TotalMinutesRead);
        }

        public async Task<ReadingProgress?> GetByIdAsync(Guid id)
        {
            return await context.ReadingProgresses
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);
        }

        public async Task SoftDeleteAsync(Guid id)
        {
            var progress = await context.ReadingProgresses
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);

            if (progress is null) return;

            progress.IsDeleted = true;
            progress.DateModified = DateTime.UtcNow;
        }
    }
}