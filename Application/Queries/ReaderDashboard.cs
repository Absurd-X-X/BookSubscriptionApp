using Application.Common.Pagenation;
using Application.Common.Repositories;
using Application.Common.Dtos;
using Domain.Enums;
using MediatR;

namespace Application.Queries
{
    public class GetReaderDashboard
    {
        public record GetReaderDashboardQuery(
            Guid UserId
            ) : IRequest<Result<GetReaderDashboardResponse>>;

        public class GetReaderDashboardHandler(
            IUserRepository userRepository,
            IReadingProgressRepository readingProgressRepository,
            IFavoriteRepository favoriteRepository,
            IBookRepository bookRepository,
            ISubscriptionRepository subscriptionRepository,
            INotificationRepository notificationRepository,
            IAuditLogRepository auditLogRepository
            ) : IRequestHandler<GetReaderDashboardQuery, Result<GetReaderDashboardResponse>>
        {
            async Task<Result<GetReaderDashboardResponse>> IRequestHandler<GetReaderDashboardQuery, Result<GetReaderDashboardResponse>>.
                Handle(GetReaderDashboardQuery request, CancellationToken cancellationToken)
            {
                var userId = request.UserId;

                var user = await userRepository.GetAsync(userId);

                if (user is null || user.Reader is null)
                {
                    return Result<GetReaderDashboardResponse>.Failure("Reader not found");
                }

                var readerId = user.Id;

                var allProgress = await readingProgressRepository.GetByReaderAsync(readerId);
                var completedBooks = await readingProgressRepository.GetCompletedBooksAsync(readerId);
                var lastReadBook = await readingProgressRepository.GetLastReadBookAsync(readerId);

                var completedCount = await readingProgressRepository.GetCompletedBookCountAsync(readerId);
                var inProgressCount = await readingProgressRepository.GetCurrentlyReadingCountAsync(readerId);

                var favorites = await favoriteRepository.GetReaderFavoritesAsync(readerId);

                var excludeIds = allProgress.Select(x => x.BookId)
                    .Concat(favorites.Select(x => x.Id))
                    .Distinct()
                    .ToList();

                var recommended = await bookRepository.GetRecommendedForReaderAsync(readerId, excludeIds, take: 5);

                var subscription = await subscriptionRepository.GetByReaderIdAsync(user.Reader.Id, isActive: true);

                var notifications = await notificationRepository.GetAllNotificationtAsync(userId);
                var unreadCount = await notificationRepository.GetUnreadCountAsync(userId);

                var activityPage = new PageRequest { Page = 1, PageSize = 4 };
                var recentActivity = await auditLogRepository.GetAsync(userId, activityPage, usePaging: true);

                var streakDays = CalculateStreak(
                    allProgress
                        .Where(x => x.LastReadDate.HasValue)
                        .Select(x => x.LastReadDate!.Value)
                        .ToList()
                );

                var readerGoalType = user.Reader.ReadingGoalType ?? ReadingGoalType.Books;
                var readerGoalTarget = user.Reader.ReadingGoalTarget ?? 50;

                int goalProgress = readerGoalType switch
                {
                    ReadingGoalType.Pages => await readingProgressRepository.GetTotalPagesReadByYearAsync(readerId, DateTime.UtcNow.Year),
                    _ => await readingProgressRepository.GetCompletedBookCountByYearAsync(readerId, DateTime.UtcNow.Year)
                };

                double goalPercent = readerGoalTarget == 0
                    ? 0
                    : Math.Round((double)goalProgress / readerGoalTarget * 100, 1);

                bool goalOnTrack = goalProgress >= readerGoalTarget;

                var response = new GetReaderDashboardResponse(
                    user.Id,
                    user.UserName,
                    user.Email,
                    user.ImageUrl,
                    user.DateCreated,

                    completedCount,
                    inProgressCount,
                    favorites.Count,
                    streakDays,

                    lastReadBook is null || lastReadBook.IsCompleted ? null : new ContinueReadingResponse(
                        lastReadBook.BookId,
                        lastReadBook.Book.Title,
                        lastReadBook.Book.Author,
                        lastReadBook.Book.BookCoverUrl,
                        lastReadBook.ProgressPercentage,
                        lastReadBook.TotalMinutesRead
                    ),

                    completedBooks
                        .OrderByDescending(x => x.LastReadDate)
                        .Take(3)
                        .Select(x => new RecentlyCompletedResponse(
                            x.BookId,
                            x.Book.Title,
                            x.Book.Author,
                            x.Book.BookCoverUrl,
                            x.LastReadDate!.Value
                        )).ToList(),

                    recommended.Select(x => new RecommendedBookResponse(
                        x.Id,
                        x.Title,
                        x.Author,
                        x.BookCoverUrl,
                        x.NoOfTimeReadByPeople
                    )).ToList(),

                    recentActivity.Items.Select(x => new ActivityResponse(
                        x.ActionType,
                        x.Icon,
                        x.Description,
                        x.Timestamp
                    )).ToList(),

                    subscription == null ? "Free Plan" : subscription.Types.TypeName,
                    subscription == null ? 0m : subscription.Types.Cost,
                    subscription == null ? (DateTime?)null : subscription.Types.ExpiryDate,

                    readerGoalType.ToString(),
                    readerGoalTarget,
                    goalProgress,
                    goalPercent,
                    goalOnTrack,
                    user.Reader.ReadingGoalDeadline,
                    user.Reader.ReadingGoalMotivation,

                    notifications
                        .OrderByDescending(x => x.DateModified)
                        .Take(3)
                        .Select(x => new NotificationResponse(
                            x.Id,
                            x.Title,
                            x.Message,
                            x.Type.ToString(),
                            x.IsRead,
                            x.DateModified
                        )).ToList(),

                    unreadCount
                );

                return Result<GetReaderDashboardResponse>.Success(response, "Retrieved");
            }

            private static int CalculateStreak(List<DateTime> lastReadDates)
            {
                var distinctDays = lastReadDates
                    .Select(d => d.Date)
                    .Distinct()
                    .OrderByDescending(d => d)
                    .ToList();

                if (!distinctDays.Any()) return 0;

                var today = DateTime.UtcNow.Date;

                if (distinctDays[0] != today && distinctDays[0] != today.AddDays(-1))
                    return 0;

                int streak = 1;
                for (int i = 0; i < distinctDays.Count - 1; i++)
                {
                    if (distinctDays[i].AddDays(-1) == distinctDays[i + 1])
                        streak++;
                    else
                        break;
                }

                return streak;
            }
        }

        public record GetReaderDashboardResponse(
            Guid UserId,
            string DisplayName,
            string Email,
            string? AvatarUrl,
            DateTime MemberSince,

            int BooksReadTotal,
            int BooksInProgress,
            int FavoriteBooksCount,
            int ReadingStreakDays,

            ContinueReadingResponse? ContinueReading,
            List<RecentlyCompletedResponse> RecentlyCompleted,
            List<RecommendedBookResponse> Recommended,
            List<ActivityResponse> RecentActivity,

            string PlanName,
            decimal MonthlyPrice,
            DateTime? RenewsOn,

            string GoalType,
            int GoalTargetValue,
            int GoalCurrentValue,
            double GoalPercentComplete,
            bool GoalIsOnTrack,
            DateTime? GoalDeadline,
            string? GoalMotivation,

            List<NotificationResponse> Notifications,
            int UnreadNotificationCount
            );

        public record ContinueReadingResponse(
            Guid BookId,
            string Title,
            string Author,
            string BookCoverUrl,
            double PercentComplete,
            int TotalMinutesRead
            );

        public record RecentlyCompletedResponse(
            Guid BookId,
            string Title,
            string Author,
            string BookCoverUrl,
            DateTime CompletedOn
            );

        public record RecommendedBookResponse(
            Guid BookId,
            string Title,
            string Author,
            string BookCoverUrl,
            int NoOfTimeReadByPeople
            );

        public record ActivityResponse(
            string ActionType,
            string Icon,
            string Description,
            DateTime Timestamp
            );

        public record NotificationResponse(
            Guid Id,
            string Title,
            string Message,
            string Type,
            bool IsRead,
            DateTime DateModified
            );
    }
}