using Application.Common.Dtos;
using Application.Common.Repositories;
using MediatR;

namespace Application.Queries
{
    public class GetMyReadingDashboard
    {
        public record GetMyReadingDashboardQuery(
            Guid UserId
            ) : IRequest<Result<GetMyReadingDashboardResponse>>;

        public class GetMyReadingDashboardHandler(
            IUserRepository userRepository,
            IReadingProgressRepository readingProgressRepository
            ) : IRequestHandler<GetMyReadingDashboardQuery, Result<GetMyReadingDashboardResponse>>
        {
            async Task<Result<GetMyReadingDashboardResponse>> IRequestHandler<GetMyReadingDashboardQuery, Result<GetMyReadingDashboardResponse>>.
                Handle(GetMyReadingDashboardQuery request, CancellationToken cancellationToken)
            {
                var user = await userRepository.GetAsync(request.UserId);

                if (user is null || user.Reader is null)
                {
                    return Result<GetMyReadingDashboardResponse>.Failure("Reader not found");
                }

                var readerId = user.Reader.Id;

                var currentlyReadingList = await readingProgressRepository.GetCurrentlyReadingAsync(readerId);
                var currentlyReading = currentlyReadingList.FirstOrDefault(); // already ordered by LastReadDate desc

                var completedBooks = await readingProgressRepository.GetCompletedBooksAsync(readerId);

                var history = await readingProgressRepository.GetReadingHistoryAsync(readerId, take: 5);

                var completedCount = await readingProgressRepository.GetCompletedBookCountAsync(readerId);
                var completedThisYear = await readingProgressRepository.GetCompletedBookCountByYearAsync(readerId, DateTime.UtcNow.Year);
                var totalMinutesRead = await readingProgressRepository.GetTotalReadingMinutesAsync(readerId);
                var currentStreak = await readingProgressRepository.GetMaxCurrentStreakAsync(readerId);
                var lastSevenDays = await readingProgressRepository.GetLastSevenDaysActivityAsync(readerId);

                var journey = await readingProgressRepository.GetJourneyChartAsync(readerId, days: 30);
                var monthlyStats = await readingProgressRepository.GetMonthlyStatsAsync(readerId);
                var genreBreakdown = await readingProgressRepository.GetGenreBreakdownAsync(readerId);

                const int annualGoalTarget = 50; // TODO: replace with a real stored target once a Reading Goal field/entity exists

                var currentlyReadingDto = currentlyReading is null
                    ? null
                    : new CurrentlyReadingDto(
                        currentlyReading.Book.Id,
                        currentlyReading.Book.Title,
                        currentlyReading.Book.Author,
                        currentlyReading.Book.BookCoverUrl,
                        currentlyReading.ProgressPercentage,
                        currentlyReading.TotalMinutesRead,
                        currentlyReading.LastReadDate
                    );

                var completedBooksDto = completedBooks
                    .OrderByDescending(x => x.LastReadDate)
                    .Take(10)
                    .Select(x => new CompletedBookDto(
                        x.Book.Id,
                        x.Book.Title,
                        x.Book.Author,
                        x.Book.BookCoverUrl,
                        x.LastReadDate
                    )).ToList();

                var historyDto = history.Select(x => new ReadingHistoryItemDto(
                    x.Book.Id,
                    x.Book.Title,
                    x.Book.Author,
                    x.Book.BookCoverUrl,
                    x.ProgressPercentage,
                    x.TotalMinutesRead,
                    x.LastReadDate
                )).ToList();

                var progressStatsDto = new ReadingProgressStatsDto(
                    completedCount,
                    totalMinutesRead
                );

                var journeyDto = journey.Select(x => new JourneyPointDto(
                    x.Date,
                    x.MinutesRead
                )).ToList();

                var streakDto = new ReadingStreakDto(
                    currentStreak,
                    lastSevenDays
                );

                var monthlyStatsDto = new MonthlyStatsDto(
                    monthlyStats.BooksRead,
                    monthlyStats.MinutesRead
                );

                var genreBreakdownDto = genreBreakdown.Select(x => new GenreBreakdownDto(
                    x.Genre,
                    x.MinutesRead
                )).ToList();

                var response = new GetMyReadingDashboardResponse(
                    currentlyReadingDto,
                    completedBooksDto,
                    historyDto,
                    progressStatsDto,
                    journeyDto,
                    streakDto,
                    monthlyStatsDto,

                    annualGoalTarget,
                    completedThisYear,
                    annualGoalTarget == 0 ? 0 : Math.Round((double)completedThisYear / annualGoalTarget * 100, 1),
                    completedThisYear >= annualGoalTarget,

                    genreBreakdownDto
                );

                return Result<GetMyReadingDashboardResponse>.Success(response, "Retrieved");
            }
        }

        public record CurrentlyReadingDto(
            Guid BookId, string Title, string Author, string BookCoverUrl,
            double ProgressPercentage, int TotalMinutesRead, DateTime? LastReadDate
        );

        public record CompletedBookDto(
            Guid BookId, string Title, string Author, string BookCoverUrl, DateTime? CompletedAt
        );

        public record ReadingHistoryItemDto(
            Guid BookId, string Title, string Author, string BookCoverUrl,
            double ProgressPercentage, int TotalMinutesRead, DateTime? LastReadDate
        );

        public record ReadingProgressStatsDto(int BooksRead, int MinutesRead);

        public record JourneyPointDto(DateTime Date, int MinutesRead);

        public record ReadingStreakDto(int CurrentStreakDays, bool[] LastSevenDays);

        public record MonthlyStatsDto(int BooksRead, int MinutesRead);

        public record GenreBreakdownDto(string Genre, int MinutesRead);

        public record GetMyReadingDashboardResponse(
            CurrentlyReadingDto? CurrentlyReading,
            List<CompletedBookDto> CompletedBooks,
            List<ReadingHistoryItemDto> ReadingHistory,
            ReadingProgressStatsDto ProgressStats,
            List<JourneyPointDto> JourneyChart,
            ReadingStreakDto Streak,
            MonthlyStatsDto MonthlyStats,

            int GoalTargetBooks,
            int GoalBooksCompleted,
            double GoalPercentComplete,
            bool GoalIsOnTrack,

            List<GenreBreakdownDto> TopGenres
        );
    }
}