using Application.Common.Dtos;
using Application.Common.Pagenation;
using Application.Common.Repositories;
using Domain.Entities;
using Domain.Enums;
using MediatR;

namespace Application.Queries
{
    public class GetReaderDetails
    {
        public record GetReaderDetailsQuery(Guid UserId) : IRequest<Result<GetReaderDetailsResponse>>;

        public class GetReaderDetailsHandler(
            IUserRepository userRepository,
            IReadingProgressRepository readingProgressRepository,
            IReviewRepository reviewRepository,
            ISubscriptionRepository subscriptionRepository,
            IWalletRepository walletRepository,
            IWalletTransactionRepository walletTransactionRepository,
            IAuditLogRepository auditLogRepository
            ) : IRequestHandler<GetReaderDetailsQuery, Result<GetReaderDetailsResponse>>
        {
            async Task<Result<GetReaderDetailsResponse>> IRequestHandler<GetReaderDetailsQuery, Result<GetReaderDetailsResponse>>.
                Handle(GetReaderDetailsQuery request, CancellationToken cancellationToken)
            {
                var user = await userRepository.GetAsync(request.UserId);

                if (user is null || user.Reader is null)
                {
                    return Result<GetReaderDetailsResponse>.Failure("User not found");
                }

                var readerId = user.Reader.Id;

                var wallet = await walletRepository.GetByUserIdAsync(user.Id);
                var subscription = await subscriptionRepository.GetByReaderIdAsync(readerId, isActive: true);

                var completedBooks = await readingProgressRepository.GetCompletedBooksAsync(readerId);
                var currentlyReadingList = await readingProgressRepository.GetCurrentlyReadingAsync(readerId);
                var currentlyReading = currentlyReadingList.FirstOrDefault();
                var readingHistory = await readingProgressRepository.GetReadingHistoryAsync(readerId, take: 10);

                var completedCount = await readingProgressRepository.GetCompletedBookCountAsync(readerId);
                var totalMinutesRead = await readingProgressRepository.GetTotalReadingMinutesAsync(readerId);
                var currentStreak = await readingProgressRepository.GetMaxCurrentStreakAsync(readerId);

                var reviewCount = await reviewRepository.CountByReaderIdAsync(readerId);

                var walletTransactionsPage = wallet is null
                    ? new PagenatedList<WalletTransaction> { Items = new List<WalletTransaction>(), TotalCount = 0 }
                    : await walletTransactionRepository.GetByWalletIdAsync(
                        wallet.Id,
                        new PageRequest { Page = 1, PageSize = 5 },
                         true);

                var totalAdded = wallet is null ? 0m : await walletTransactionRepository.GetTotalByTypeAsync(wallet.Id, TransactionType.Credit);

                // ASSUMPTION: "Debit" is the enum member for money leaving the wallet — rename if your actual TransactionType value differs
                var totalSpent = wallet is null ? 0m : await walletTransactionRepository.GetTotalByTypeAsync(wallet.Id, TransactionType.Debit);

                var activityPage = new PageRequest { Page = 1, PageSize = 10 };
                var auditLogs = await auditLogRepository.GetAsync(user.Id, activityPage, usePaging: true);

                // ── Header ──
                var headerDto = new UserHeaderDto(
                    user.Id,
                    user.UserName,
                    user.Email,
                    user.ImageUrl,
                    user.Role,
                    user.IsVerified,
                    true,  // MOCK — no 2FA field on User yet
                    user.DateCreated,
                    null,  // MOCK — no LastActive tracking yet
                    !user.IsDeleted
                );

                // ── Overview tab ──
                var overviewDto = new OverviewTabDto(
                    completedCount,
                    currentStreak,
                    reviewCount,
                    totalMinutesRead,
                    new ReadingProgressBreakdownDto(
                        completedCount,
                        currentlyReadingList.Count,
                        completedCount + currentlyReadingList.Count
                    ),
                    auditLogs.Items.Take(5).Select(x => new ActivityItemDto(
                        x.ActionType,
                        x.Icon,
                        x.Description,
                        x.Timestamp
                    )).ToList(),
                    subscription == null ? "Free Plan" : subscription.Types.TypeName,
                    subscription != null,
                    subscription == null ? (DateTime?)null : subscription.Types.ExpiryDate,
                    wallet?.Balance ?? 0m,
                    wallet?.Transactions.Count(x => !x.IsDeleted) ?? 0
                );

                // ── Subscription tab ──
                var subscriptionDto = new SubscriptionTabDto(
                    subscription == null ? "Free Plan" : subscription.Types.TypeName,
                    subscription == null ? 0m : subscription.Types.Cost,
                    subscription == null ? BillingCycle.Monthly : subscription.Types.Cycle,
                    subscription?.IsActive ?? false,
                    subscription?.DateCreated,
                    subscription == null ? (DateTime?)null : subscription.Types.SubscriptionDate,
                    subscription == null ? (DateTime?)null : subscription.Types.ExpiryDate,
                    subscription?.AutoRenewal ?? false
                );

                // ── Wallet tab ──
                var walletDto = new WalletTabDto(
                    wallet?.Balance ?? 0m,
                    totalAdded,
                    totalSpent,
                    wallet?.Transactions.Count(x => !x.IsDeleted) ?? 0,
                    walletTransactionsPage.Items.Select(x => new WalletTransactionDto(
                        x.Id,
                        x.DateCreated,
                        x.Type.ToString(),
                        x.Description,
                        x.Balance,
                        x.Status.ToString(),
                        x.BalanceAfter
                    )).ToList(),
                    walletTransactionsPage.TotalCount
                );

                // ── Reading Activity tab ──
                var readingActivityDto = new ReadingActivityTabDto(
                    completedCount + currentlyReadingList.Count,
                    completedCount,
                    currentlyReadingList.Count,
                    currentStreak,
                    readingHistory.Select(x => new ReadingLogItemDto(
                        x.Book.Id,
                        x.Book.Title,
                        x.Book.Author,
                        x.Book.BookCoverUrl,
                        x.ProgressPercentage,
                        x.TotalMinutesRead,
                        x.LastReadDate,
                        x.IsCompleted
                    )).ToList(),
                    currentlyReading is null ? null : new CurrentBookDto(
                        currentlyReading.Book.Id,
                        currentlyReading.Book.Title,
                        currentlyReading.Book.Author,
                        currentlyReading.Book.BookCoverUrl,
                        currentlyReading.ProgressPercentage
                    )
                );

                // ── Audit Logs tab ──
                var auditLogsDto = new AuditLogsTabDto(
                    auditLogs.Items.Select(x => new ActivityItemDto(
                        x.ActionType,
                        x.Icon,
                        x.Description,
                        x.Timestamp
                    )).ToList(),
                    auditLogs.TotalCount
                );

                var response = new GetReaderDetailsResponse(
                    headerDto,
                    overviewDto,
                    subscriptionDto,
                    walletDto,
                    readingActivityDto,
                    auditLogsDto
                );

                return Result<GetReaderDetailsResponse>.Success(response, "Retrieved");
            }
        }

        public record UserHeaderDto(
            Guid UserId, string UserName, string Email, string? AvatarUrl,
            string Role, bool IsEmailVerified, bool TwoFactorEnabled,
            DateTime JoinedDate, DateTime? LastActive, bool IsActive
        );

        public record ReadingProgressBreakdownDto(int Completed, int InProgress, int Total);

        public record ActivityItemDto(string ActionType, string Icon, string Description, DateTime Timestamp);

        public record OverviewTabDto(
            int BooksRead, int ReadingStreakDays, int ReviewCount, int TotalMinutesRead,
            ReadingProgressBreakdownDto ReadingProgress,
            List<ActivityItemDto> RecentActivity,
            string PlanName, bool IsSubscribed, DateTime? SubscriptionRenewsOn,
            decimal WalletBalance, int WalletTransactionCount
        );

        public record SubscriptionTabDto(
            string PlanName, decimal Cost, BillingCycle Cycle, bool IsActive,
            DateTime? PurchasedOn, DateTime? CurrentPeriodStart, DateTime? CurrentPeriodEnd,
            bool AutoRenew
        );

        public record WalletTransactionDto(
            Guid Id, DateTime DateCreated, string Type, string Description,
            decimal Amount, string Status, decimal BalanceAfter
        );

        public record WalletTabDto(
            decimal CurrentBalance, decimal TotalAdded, decimal TotalSpent, int TotalTransactions,
            List<WalletTransactionDto> RecentTransactions, long TransactionTotalCount
        );

        public record ReadingLogItemDto(
            Guid BookId, string Title, string Author, string BookCoverUrl,
            double ProgressPercentage, int TotalMinutesRead, DateTime? LastReadDate, bool IsCompleted
        );

        public record CurrentBookDto(Guid BookId, string Title, string Author, string BookCoverUrl, double ProgressPercentage);

        public record ReadingActivityTabDto(
            int BooksStarted, int BooksCompleted, int CurrentlyReading, int ReadingStreakDays,
            List<ReadingLogItemDto> ReadingLogs,
            CurrentBookDto? CurrentBook
        );

        public record AuditLogsTabDto(List<ActivityItemDto> Logs, long TotalCount);

        public record GetReaderDetailsResponse(
            UserHeaderDto Header,
            OverviewTabDto Overview,
            SubscriptionTabDto Subscription,
            WalletTabDto Wallet,
            ReadingActivityTabDto ReadingActivity,
            AuditLogsTabDto AuditLogs
        );
    }
}