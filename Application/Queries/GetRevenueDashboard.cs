using Application.Common.Dtos;
using Application.Common.Pagenation;
using Application.Common.Repositories;
using MediatR;
using static Application.Queries.GetRevenueDashboard.GetRevenueDashboardHandler;

namespace Application.Queries
{
    public class GetRevenueDashboard
    {
        public record GetRevenueDashboardQuery(Guid UserId, int Page, int PageSize, Guid libraryId)
            : IRequest<Result<RevenueDashboardResponse>>;

        public class GetRevenueDashboardHandler(
            IWalletRepository walletRepository,
            IWalletTransactionRepository walletTransactionRepository,
            IBookRepository bookRepository
            )
            : IRequestHandler<GetRevenueDashboardQuery, Result<RevenueDashboardResponse>>
        {
            public async Task<Result<RevenueDashboardResponse>> Handle(
                GetRevenueDashboardQuery request,
                CancellationToken cancellationToken)
            {
                var wallet = await walletRepository.GetByUserIdAsync(request.UserId);
                if (wallet is null)
                    return Result<RevenueDashboardResponse>.Failure("Wallet not found");

                var now = DateTime.UtcNow;
                var rangeStart = new DateTime(now.Year, now.Month, 1);
                var rangeEnd = rangeStart.AddMonths(1).AddDays(-1);
                var prevRangeStart = rangeStart.AddMonths(-1);
                var prevRangeEnd = rangeStart.AddDays(-1);

                // --- Total earnings (current vs previous month) ---
                var totalEarnings = await walletTransactionRepository
                    .GetSumByDateRangeAsync(wallet.Id, rangeStart, rangeEnd);

                var prevTotalEarnings = await walletTransactionRepository
                    .GetSumByDateRangeAsync(wallet.Id, prevRangeStart, prevRangeEnd);
                var totalEarningsGrowth = prevTotalEarnings == 0
                    ? 0
                    : Math.Round((totalEarnings - prevTotalEarnings) / prevTotalEarnings * 100, 1);

                // --- Pending payout / last payout ---
                var pendingPayout = await walletTransactionRepository.GetPendingPayoutAmountAsync(wallet.Id);
                var unpaidMonths = await walletTransactionRepository.GetUnpaidMonthsCountAsync(wallet.Id);
                var lastPayout = await walletTransactionRepository.GetLastPaidPayoutAsync(wallet.Id);

                // --- Payout status overview (replaces old category breakdown) ---
                var payoutStatusOverview = await walletTransactionRepository.GetPayoutStatusOverviewAsync(wallet.Id);

                // --- Books sold this month (vs previous) ---
                var booksSoldThisMonth = await walletTransactionRepository
                    .GetSumByDateRangeAsync(wallet.Id, rangeStart, rangeEnd);
                var booksSoldPrevMonth = await walletTransactionRepository
                    .GetSumByDateRangeAsync(wallet.Id, prevRangeStart, prevRangeEnd);
                var booksSoldGrowth = booksSoldPrevMonth == 0
                    ? 0
                    : Math.Round((decimal)(booksSoldThisMonth - booksSoldPrevMonth) / booksSoldPrevMonth * 100, 1);

                // --- Best performing book ---
                var books = await bookRepository.GetByLibraryIdAsync(request.libraryId, new PageRequest
                {
                    Page = request.Page,
                    PageSize = request.PageSize,
                }, false);

                var bestBook = books.Items.OrderByDescending(x => x.NoOfTimeReadByPeople).FirstOrDefault();

                // --- Top earning books list ---
                var topBooks = books.Items.OrderByDescending(x => x.NoOfTimeReadByPeople).Take(5);

                // --- Trend data (last 6 months) ---
                var trendMonths = Enumerable.Range(0, 6)
                    .Select(i => rangeStart.AddMonths(-5 + i))
                    .ToList();
                var trendLabels = trendMonths.Select(m => m.ToString("MMM yyyy")).ToList();
                var trendData = new List<decimal>();
                foreach (var month in trendMonths)
                {
                    var monthEnd = month.AddMonths(1).AddDays(-1);
                    var sum = await walletTransactionRepository.GetSumByDateRangeAsync(wallet.Id, month, monthEnd);
                    trendData.Add(sum);
                }

                // --- Recent transactions ---
                var transactions = await walletTransactionRepository
                    .GetByWalletIdAsync(wallet.Id, new PageRequest { Page = request.Page, PageSize = request.PageSize }, true);

                var recentTransactions = new PagenatedList<GetTransactionHistory.GetTransactionHistoryResponse>
                {
                    Items = transactions.Items.Select(t => new GetTransactionHistory.GetTransactionHistoryResponse(
                        t.Id, t.Balance, t.Type.ToString(), t.Status.ToString(), t.Description,
                        t.BalanceBefore, t.BalanceAfter, t.DateCreated)).ToList(),
                    Page = request.Page,
                    PageSize = request.PageSize,
                    TotalCount = transactions.TotalCount
                };

                var response = new RevenueDashboardResponse(
                    RangeStart: rangeStart,
                    RangeEnd: rangeEnd,
                    TotalEarnings: totalEarnings,
                    TotalEarningsGrowthPercent: totalEarningsGrowth,
                    PendingPayout: pendingPayout,
                    UnpaidMonthsCount: unpaidMonths,
                    LastPayoutAmount: lastPayout?.Balance ?? 0,
                    LastPayoutDate: lastPayout?.DateCreated,
                    BooksSoldThisMonth: booksSoldThisMonth,
                    BooksSoldGrowthPercent: booksSoldGrowth,
                    CurrentMonthEarnings: totalEarnings,
                    BestPerformingBook: bestBook is null ? null : new BestBookDto(bestBook.Title, 200000000),
                    PayoutStatusOverview: payoutStatusOverview,
                    TopEarningBooks: topBooks.Select(b => new TopBookDto(b.Title, b.BookCoverUrl, 150000)).ToList(),
                    TrendLabels: trendLabels,
                    TrendData: trendData,
                    RecentTransactions: recentTransactions
                );

                return Result<RevenueDashboardResponse>.Success(response, "Success");
            }

            public record RevenueDashboardResponse(
                DateTime RangeStart,
                DateTime RangeEnd,
                decimal TotalEarnings,
                decimal TotalEarningsGrowthPercent,
                decimal PendingPayout,
                int UnpaidMonthsCount,
                decimal LastPayoutAmount,
                DateTime? LastPayoutDate,
                decimal BooksSoldThisMonth,
                decimal BooksSoldGrowthPercent,
                decimal CurrentMonthEarnings,
                BestBookDto? BestPerformingBook,
                PayoutStatusOverviewDto PayoutStatusOverview,
                List<TopBookDto> TopEarningBooks,
                List<string> TrendLabels,
                List<decimal> TrendData,
                PagenatedList<GetTransactionHistory.GetTransactionHistoryResponse> RecentTransactions
            );

            public record BestBookDto(string Title, decimal Earnings);
            public record PayoutStatusOverviewDto(int PaidMonthsCount, int PendingMonthsCount, decimal TotalPaidAmount, decimal TotalPendingAmount); 
            public record TopBookDto(string Title, string? CoverImageUrl, decimal Earnings);
        }
    }
}