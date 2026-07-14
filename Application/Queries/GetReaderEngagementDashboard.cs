using Application.Common.Repositories;
using Domain.Entities;
using MediatR;

namespace Application.Features.ReaderEngagement.Queries.GetReaderEngagementDashboard
{
    // ── Query ──────────────────────────────────────────────
    public sealed record GetReaderEngagementDashboardQuery(Guid ReaderId, string ChartRange = "8w")
        : IRequest<ReaderEngagementVm>;

    // ── View Models ────────────────────────────────────────
    public sealed class ReaderEngagementVm
    {
        public ReviewSummaryVm ReviewSummary { get; init; } = new();
        public List<RecentReviewVm> RecentReviews { get; init; } = [];
        public List<NotificationVm> Notifications { get; init; } = [];
        public int UnreadNotificationCount { get; init; }
        public AnalyticsOverviewVm Analytics { get; init; } = new();
    }

    public sealed class ReviewSummaryVm
    {
        public double AverageRating { get; init; }
        public int TotalReviews { get; init; }
        public List<RatingBarVm> Distribution { get; init; } = [];
    }

    public sealed class RatingBarVm
    {
        public int Stars { get; init; }
        public int Count { get; init; }
        public double Percentage { get; init; }
    }

    public sealed class RecentReviewVm
    {
        public Guid ReviewId { get; init; }
        public Guid BookId { get; init; }
        public string BookTitle { get; init; } = default!;
        public string BookAuthor { get; init; } = default!;
        public int Rating { get; init; }
        public string Comment { get; init; } = default!;
        public DateTime DateCreated { get; init; }
    }

    public sealed class NotificationVm
    {
        public Guid Id { get; init; }
        public string Title { get; init; } = default!;
        public string Message { get; init; } = default!;
        public bool IsRead { get; init; }
        public DateTime DateCreated { get; init; }
        // TODO: expose Type/RefType here once NotificationType is confirmed,
        // so the view can map to a real icon instead of a static bell.
    }

    public sealed class TrendVm
    {
        public double PercentChange { get; init; }
        public bool IsUp => PercentChange >= 0;
    }

    public sealed class AnalyticsOverviewVm
    {
        public int BooksReadThisYear { get; init; }
        public int TotalPagesRead { get; init; }
        public int TotalMinutesRead { get; init; }
        public double AverageRatingGiven { get; init; }
        public List<WeeklyChartPointVm> Chart { get; init; } = [];

        public TrendVm? BooksReadTrend { get; init; }
        public TrendVm? PagesReadTrend { get; init; }
        public TrendVm? ReadingTimeTrend { get; init; }
        public TrendVm? AvgRatingTrend { get; init; }
    }

    public sealed class WeeklyChartPointVm
    {
        public string Label { get; init; } = default!;
        public double Hours { get; init; }
    }

    // ── Handler ────────────────────────────────────────────
    public sealed class GetReaderEngagementDashboardQueryHandler(
        IReviewRepository reviewRepository,
        INotificationRepository notificationRepository,
        IReadingProgressRepository readingProgressRepository)
        : IRequestHandler<GetReaderEngagementDashboardQuery, ReaderEngagementVm>
    {
        public async Task<ReaderEngagementVm> Handle(GetReaderEngagementDashboardQuery request, CancellationToken ct)
        {
            // ── Review summary ──
            var avgGiven = await reviewRepository.GetAverageRatingGivenByReaderAsync(request.ReaderId);
            var totalReviews = await reviewRepository.CountByReaderIdAsync(request.ReaderId);
            var distribution = await reviewRepository.GetRatingDistributionByReaderAsync(request.ReaderId);

            var distributionVms = distribution
                .OrderByDescending(kv => kv.Key)
                .Select(kv => new RatingBarVm
                {
                    Stars = kv.Key,
                    Count = kv.Value,
                    Percentage = totalReviews > 0 ? Math.Round((double)kv.Value / totalReviews * 100, 0) : 0
                })
                .ToList();

            var recentReviews = await reviewRepository.GetByReaderIdAsync(request.ReaderId, 3);
            var recentReviewVms = recentReviews.Select(r => new RecentReviewVm
            {
                ReviewId = r.Id,
                BookId = r.BookId,
                BookTitle = r.Book.Title,
                BookAuthor = r.Book.Author,
                Rating = r.Rating,
                Comment = r.Comment,
                DateCreated = r.DateCreated
            }).ToList();

            // ── Notifications ──
            var notifications = await notificationRepository.GetAllNotificationtAsync(request.ReaderId);
            var notificationVms = notifications
                .Where(n => !n.IsDeleted && !n.IsArchived)
                .OrderByDescending(n => n.DateCreated)
                .Take(10)
                .Select(n => new NotificationVm
                {
                    Id = n.Id,
                    Title = n.Title,
                    Message = n.Message,
                    IsRead = n.IsRead,
                    DateCreated = n.DateCreated
                })
                .ToList();

            var unreadCount = await notificationRepository.GetUnreadCountAsync(request.ReaderId);

            // ── Analytics: current totals ──
            var totalPages = await readingProgressRepository.GetTotalPagesReadAsync(request.ReaderId);
            var totalMinutes = await readingProgressRepository.GetTotalReadingMinutesAsync(request.ReaderId);

            var currentYear = DateTime.UtcNow.Year;
            var lastYear = currentYear - 1;

            var booksThisYear = await readingProgressRepository.GetCompletedBookCountByYearAsync(request.ReaderId, currentYear);
            var booksLastYear = await readingProgressRepository.GetCompletedBookCountByYearAsync(request.ReaderId, lastYear);

            var pagesThisYear = await readingProgressRepository.GetTotalPagesReadByYearAsync(request.ReaderId, currentYear);
            var pagesLastYear = await readingProgressRepository.GetTotalPagesReadByYearAsync(request.ReaderId, lastYear);

            var minutesThisYear = await readingProgressRepository.GetTotalMinutesReadByYearAsync(request.ReaderId, currentYear);
            var minutesLastYear = await readingProgressRepository.GetTotalMinutesReadByYearAsync(request.ReaderId, lastYear);

            var ratingThisYear = await reviewRepository.GetAverageRatingGivenByReaderInYearAsync(request.ReaderId, currentYear);
            var ratingLastYear = await reviewRepository.GetAverageRatingGivenByReaderInYearAsync(request.ReaderId, lastYear);

            // ── Chart ──
            var days = request.ChartRange switch
            {
                "4w" => 28,
                "12w" => 84,
                "6m" => 182,
                _ => 56 // "8w"
            };

            var journeyPoints = await readingProgressRepository.GetJourneyChartAsync(request.ReaderId, days);
            var chart = BuildWeeklyBuckets(journeyPoints, days);

            return new ReaderEngagementVm
            {
                ReviewSummary = new ReviewSummaryVm
                {
                    AverageRating = Math.Round(avgGiven, 1),
                    TotalReviews = totalReviews,
                    Distribution = distributionVms
                },
                RecentReviews = recentReviewVms,
                Notifications = notificationVms,
                UnreadNotificationCount = unreadCount,
                Analytics = new AnalyticsOverviewVm
                {
                    BooksReadThisYear = booksThisYear,
                    TotalPagesRead = totalPages,
                    TotalMinutesRead = totalMinutes,
                    AverageRatingGiven = Math.Round(avgGiven, 1),
                    Chart = chart,
                    BooksReadTrend = BuildTrend(booksThisYear, booksLastYear),
                    PagesReadTrend = BuildTrend(pagesThisYear, pagesLastYear),
                    ReadingTimeTrend = BuildTrend(minutesThisYear, minutesLastYear),
                    AvgRatingTrend = BuildTrend(ratingThisYear, ratingLastYear)
                }
            };
        }

        // Buckets daily minute totals into weekly hour totals, oldest → newest.
        private static List<WeeklyChartPointVm> BuildWeeklyBuckets(
            List<JourneyChartPoint> points, int totalDays)
        {
            var start = DateTime.UtcNow.Date.AddDays(-(totalDays - 1));
            var weekCount = totalDays / 7;
            var buckets = new List<WeeklyChartPointVm>();

            for (int w = 0; w < weekCount; w++)
            {
                var weekStart = start.AddDays(w * 7);
                var weekEnd = weekStart.AddDays(6);

                var minutesInWeek = points
                    .Where(p => p.Date >= weekStart && p.Date <= weekEnd)
                    .Sum(p => p.MinutesRead);

                buckets.Add(new WeeklyChartPointVm
                {
                    Label = weekStart.ToString("MMM d"),
                    Hours = Math.Round(minutesInWeek / 60.0, 1)
                });
            }

            return buckets;
        }

        private static TrendVm? BuildTrend(double current, double previous)
        {
            if (previous <= 0) return null;

            var change = (current - previous) / previous * 100;
            return new TrendVm { PercentChange = Math.Round(change, 0) };
        }
    }
}