using Application.Common.Dtos;
using Application.Common.Pagenation;
using Application.Common.Repositories;
using MediatR;
using static Application.Queries.GetLibraryAnalytics.GetLibraryAnalyticsHandler;

namespace Application.Queries
{
    public class GetLibraryAnalytics
    {
        public record GetLibraryAnalyticsQuery(Guid UserId, Guid LibraryId, DateTime StartDate, DateTime EndDate, int Page, int PageSize)
            : IRequest<Result<LibraryAnalyticsResponse>>;

        public class GetLibraryAnalyticsHandler(
            IBookRepository bookRepository,
            IReviewRepository reviewRepository,
            IReadingProgressRepository readingProgressRepository
            )
            : IRequestHandler<GetLibraryAnalyticsQuery, Result<LibraryAnalyticsResponse>>
        {
            public async Task<Result<LibraryAnalyticsResponse>> Handle(
                GetLibraryAnalyticsQuery request,
                CancellationToken cancellationToken)
            {
                var start = request.StartDate;
                var end = request.EndDate;
                var prevStart = start.AddMonths(-1);
                var prevEnd = start.AddDays(-1);

                var books = await bookRepository.GetByLibraryIdAsync(request.LibraryId, new PageRequest
                {
                    Page = request.Page,
                    PageSize = request.PageSize,
                }, false);

                var reviews = await reviewRepository.GetByLibraryIdAsync(new PageRequest
                {
                    Page = request.Page,
                    PageSize = request.PageSize,
                }, false, request.LibraryId);

                var progress = await readingProgressRepository.GetByLibraryIdAsync(request.LibraryId, start, end);
                var prevProgress = await readingProgressRepository.GetByLibraryIdAsync(request.LibraryId, prevStart, prevEnd);

                // --- Readers ---
                var totalReaders = progress.Select(p => p.ReaderId).Distinct().Count();
                var prevTotalReaders = prevProgress.Select(p => p.ReaderId).Distinct().Count();
                var totalReadersGrowth = PercentChange(prevTotalReaders, totalReaders);

                var activeReaders = progress.Where(p => p.Percentage > 0).Select(p => p.ReaderId).Distinct().Count();
                var prevActiveReaders = prevProgress.Where(p => p.Percentage > 0).Select(p => p.ReaderId).Distinct().Count();
                var activeReadersGrowth = PercentChange(prevActiveReaders, activeReaders);

                // --- Ratings (from Review.Rating) ---
                var allRatings = reviews.Items.Select(r => r.Rating).ToList();
                var avgRating = allRatings.Any() ? allRatings.Average() : 0;
                var prevRatings = reviews.Items
                    .Where(r => r.DateCreated >= prevStart && r.DateCreated <= prevEnd)
                    .Select(r => r.Rating).ToList();
                var prevAvgRating = prevRatings.Any() ? prevRatings.Average() : 0;
                var avgRatingChange = Math.Round((decimal)(avgRating - prevAvgRating), 1);

                // --- Reads / Completion (from ReadingProgress) ---
                var totalReads = progress.Count;
                var completedReads = progress.Count(p => p.IsCompleted);
                var abandonedReads = totalReads - completedReads;
                var completionRate = PctOfTotal(completedReads, totalReads);

                var prevTotalReads = prevProgress.Count;
                var prevCompletedReads = prevProgress.Count(p => p.IsCompleted);
                var prevAbandonedReads = prevTotalReads - prevCompletedReads;
                var prevCompletionRate = PctOfTotal(prevCompletedReads, prevTotalReads);

                var totalReadsGrowth = PercentChange(prevTotalReads, totalReads);
                var completedReadsGrowth = PercentChange(prevCompletedReads, completedReads);
                var abandonedReadsGrowth = PercentChange(prevAbandonedReads, abandonedReads);
                var completionRateGrowth = completionRate - prevCompletionRate;

                // --- Best / fastest growing book (from NoOfTimeReadByPeople) ---
                var bestBook = books.Items.OrderByDescending(x => x.NoOfTimeReadByPeople).FirstOrDefault();
                var bestBookProgress = bestBook is null ? new List<Domain.Entities.ReadingProgress>()
                    : progress.Where(p => p.BookId == bestBook.Id).ToList();
                var bestBookCompletionPct = PctOfTotal(bestBookProgress.Count(p => p.IsCompleted), bestBookProgress.Count);

                // Fastest growing: compare NoOfTimeReadByPeople against progress-entry growth this period vs last
                var fastestGrowingBook = books.Items
                    .Select(b =>
                    {
                        var currentCount = progress.Count(p => p.BookId == b.Id);
                        var prevCount = prevProgress.Count(p => p.BookId == b.Id);
                        var growth = prevCount > 0 ? (decimal)(currentCount - prevCount) / prevCount * 100 : 0;
                        return new { Book = b, Growth = growth, PrevCount = prevCount };
                    })
                    .Where(x => x.PrevCount > 0)
                    .OrderByDescending(x => x.Growth)
                    .FirstOrDefault();

                // --- Most active day (from ReadingProgress.LastReadAt) ---
                var mostActiveDayGroup = progress
                    .GroupBy(p => p.LastReadAt.DayOfWeek)
                    .OrderByDescending(g => g.Select(p => p.ReaderId).Distinct().Count())
                    .FirstOrDefault();

                // --- Funnel (only stages derivable from Percentage/IsCompleted) ---
                var funnelOpened = totalReads;
                var funnelStarted = progress.Count(p => p.Percentage > 0);
                var funnelReached50 = progress.Count(p => p.Percentage >= 50);
                var funnelReached90 = progress.Count(p => p.Percentage >= 90);

                var funnel = new List<FunnelStepDto>
                {
                    new("Book Opened", funnelOpened, 100),
                    new("Started Reading", funnelStarted, PctOfTotal(funnelStarted, funnelOpened)),
                    new("Reached 50%", funnelReached50, PctOfTotal(funnelReached50, funnelOpened)),
                    new("Reached 90%", funnelReached90, PctOfTotal(funnelReached90, funnelOpened)),
                    new("Completed", completedReads, PctOfTotal(completedReads, funnelOpened))
                };

                // --- Top books ---
                var topBooks = new List<TopBookDto>();
                foreach (var book in books.Items.OrderByDescending(x => x.NoOfTimeReadByPeople).Take(5))
                {
                    var bookProgress = progress.Where(p => p.BookId == book.Id).ToList();
                    var completionPct = PctOfTotal(bookProgress.Count(p => p.IsCompleted), bookProgress.Count);
                    var bookRatings = reviews.Items.Where(r => r.BookId == book.Id).Select(r => r.Rating).ToList();
                    var rating = bookRatings.Any() ? Math.Round(bookRatings.Average(), 1) : 0;
                    topBooks.Add(new TopBookDto(book.Title, book.BookCoverUrl, book.NoOfTimeReadByPeople, completionPct, rating));
                }

                // --- Ratings distribution ---
                var ratingsDistribution = new RatingsDistributionDto(
                    FiveStarPct: PctOfTotal(allRatings.Count(r => r == 5), allRatings.Count),
                    FourStarPct: PctOfTotal(allRatings.Count(r => r == 4), allRatings.Count),
                    ThreeStarPct: PctOfTotal(allRatings.Count(r => r == 3), allRatings.Count),
                    TwoStarPct: PctOfTotal(allRatings.Count(r => r == 2), allRatings.Count),
                    OneStarPct: PctOfTotal(allRatings.Count(r => r == 1), allRatings.Count)
                );

                // --- Daily trend (readers active per day, from LastReadAt) ---
                var trend = progress
                    .GroupBy(p => p.LastReadAt.Date)
                    .OrderBy(g => g.Key)
                    .Select(g => new { Date = g.Key, Count = g.Select(p => p.ReaderId).Distinct().Count() })
                    .ToList();

                // --- NOT DERIVABLE from current entities — stubbed until tracked ---
                // Requires session start/end timestamps: avg reading time, avg session length, pages/session
                double avgReadingMinutes = 0;
                double prevAvgReadingMinutes = 0;
                double pagesPerSession = 0;
                double prevPagesPerSession = 0;
                double avgDailyReaders = trend.Any() ? trend.Average(t => t.Count) : 0;
                double prevAvgDailyReaders = 0;
                double returnReaderRate = 0; // requires session history per reader across periods
                double prevReturnReaderRate = 0;

                // Requires DeviceType field on ReadingProgress or a session entity
                var deviceBreakdown = new DeviceBreakdownDto(MobilePct: 0, DesktopPct: 0, TabletPct: 0);

                // Requires ReaderId -> Reader.DateOfBirth/Age on Reader entity (not shown yet)
                var demographics = new DemographicsDto(Age18To24Pct: 0, Age25To34Pct: 0, Age35To44Pct: 0, Age45PlusPct: 0);

                // Requires session duration; reusing Percentage buckets against IsCompleted is not equivalent — left zeroed
                var timeDistribution = new ReadingTimeDistributionDto(Under30MinPct: 0, Min30To60Pct: 0, Hour1To2Pct: 0, Hour2To3Pct: 0, Over3HourPct: 0);

                // Requires Reader.Country or geolocation on progress/session
                var topLocations = new List<LocationStatDto>();

                var response = new LibraryAnalyticsResponse(
                    PeriodStart: start,
                    PeriodEnd: end,
                    TotalReaders: totalReaders,
                    TotalReadersGrowthPercent: totalReadersGrowth,
                    ActiveReaders: activeReaders,
                    ActiveReadersGrowthPercent: activeReadersGrowth,
                    AverageRating: Math.Round(avgRating, 1),
                    AverageRatingChange: avgRatingChange,
                    AverageCompletionRate: completionRate,
                    AverageCompletionRateGrowthPercent: completionRateGrowth,

                    TrendLabels: trend.Select(t => t.Date.ToString("MMM d")).ToList(),
                    TrendData: trend.Select(t => t.Count).ToList(),

                    BestPerformingBookTitle: bestBook?.Title,
                    BestPerformingBookReads: bestBook?.NoOfTimeReadByPeople ?? 0,
                    BestPerformingBookCompletionPercent: bestBookCompletionPct,
                    MostActiveDay: mostActiveDayGroup?.Key.ToString(),
                    MostActiveDayReaders: mostActiveDayGroup?.Select(p => p.ReaderId).Distinct().Count() ?? 0,
                    AverageReadingTimeMinutes: avgReadingMinutes,
                    AverageReadingTimeGrowthPercent: PercentChange((decimal)prevAvgReadingMinutes, (decimal)avgReadingMinutes),
                    FastestGrowingBookTitle: fastestGrowingBook?.Book.Title,
                    FastestGrowingBookGrowthPercent: fastestGrowingBook?.Growth ?? 0,

                    TotalReads: totalReads,
                    TotalReadsGrowthPercent: totalReadsGrowth,
                    CompletedReads: completedReads,
                    CompletedReadsGrowthPercent: completedReadsGrowth,
                    AbandonedReads: abandonedReads,
                    AbandonedReadsGrowthPercent: abandonedReadsGrowth,

                    TimeDistribution: timeDistribution,
                    Funnel: funnel,
                    TopBooks: topBooks,
                    Demographics: demographics,
                    Devices: deviceBreakdown,

                    AvgSessionTimeMinutes: avgReadingMinutes,
                    AvgSessionTimeGrowthPercent: PercentChange((decimal)prevAvgReadingMinutes, (decimal)avgReadingMinutes),
                    PagesPerSession: pagesPerSession,
                    PagesPerSessionGrowthPercent: PercentChange((decimal)prevPagesPerSession, (decimal)pagesPerSession),
                    AvgDailyReaders: avgDailyReaders,
                    AvgDailyReadersGrowthPercent: PercentChange((decimal)prevAvgDailyReaders, (decimal)avgDailyReaders),
                    ReturnReaderRatePercent: returnReaderRate,
                    ReturnReaderRateGrowthPercent: returnReaderRate - prevReturnReaderRate,

                    Ratings: ratingsDistribution,
                    TopLocations: topLocations
                );

                return Result<LibraryAnalyticsResponse>.Success(response, "Success");
            }

            private static int PctOfTotal(int part, int total) =>
                total > 0 ? (int)Math.Round((double)part / total * 100) : 0;

            private static decimal PercentChange(int previous, int current) =>
                previous > 0 ? Math.Round((decimal)(current - previous) / previous * 100, 1) : 0;

            private static decimal PercentChange(decimal previous, decimal current) =>
                previous > 0 ? Math.Round((current - previous) / previous * 100, 1) : 0;

            

            public record FunnelStepDto(string Label, int Count, int Percentage);
            public record TopBookDto(string Title, string? CoverImageUrl, int Reads, int CompletionPercent, double Rating);
            public record ReadingTimeDistributionDto(int Under30MinPct, int Min30To60Pct, int Hour1To2Pct, int Hour2To3Pct, int Over3HourPct);
            public record DemographicsDto(int Age18To24Pct, int Age25To34Pct, int Age35To44Pct, int Age45PlusPct);
            public record DeviceBreakdownDto(int MobilePct, int DesktopPct, int TabletPct);
            public record RatingsDistributionDto(int FiveStarPct, int FourStarPct, int ThreeStarPct, int TwoStarPct, int OneStarPct);
            public record LocationStatDto(string CountryName, int ReaderCount, decimal Percentage);
        }

        public record LibraryAnalyticsResponse(
                DateTime PeriodStart,
                DateTime PeriodEnd,

                int TotalReaders,
                decimal TotalReadersGrowthPercent,
                int ActiveReaders,
                decimal ActiveReadersGrowthPercent,
                double AverageRating,
                decimal AverageRatingChange,
                int AverageCompletionRate,
                decimal AverageCompletionRateGrowthPercent,

                List<string> TrendLabels,
                List<int> TrendData,

                string? BestPerformingBookTitle,
                int BestPerformingBookReads,
                int BestPerformingBookCompletionPercent,
                string? MostActiveDay,
                int MostActiveDayReaders,
                double AverageReadingTimeMinutes,
                decimal AverageReadingTimeGrowthPercent,
                string? FastestGrowingBookTitle,
                decimal FastestGrowingBookGrowthPercent,

                int TotalReads,
                decimal TotalReadsGrowthPercent,
                int CompletedReads,
                decimal CompletedReadsGrowthPercent,
                int AbandonedReads,
                decimal AbandonedReadsGrowthPercent,

                ReadingTimeDistributionDto TimeDistribution,
                List<FunnelStepDto> Funnel,
                List<TopBookDto> TopBooks,
                DemographicsDto Demographics,
                DeviceBreakdownDto Devices,

                double AvgSessionTimeMinutes,
                decimal AvgSessionTimeGrowthPercent,
                double PagesPerSession,
                decimal PagesPerSessionGrowthPercent,
                double AvgDailyReaders,
                decimal AvgDailyReadersGrowthPercent,
                double ReturnReaderRatePercent,
                double ReturnReaderRateGrowthPercent,

                RatingsDistributionDto Ratings,
                List<LocationStatDto> TopLocations
            );
    }
}