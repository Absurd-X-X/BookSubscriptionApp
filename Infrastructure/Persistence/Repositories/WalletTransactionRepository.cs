using Application.Common.Pagenation;
using Application.Common.Repositories;
using Domain.Entities;
using Domain.Enums;
using Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;
using System.Transactions;
using static Application.Queries.GetRevenueDashboard.GetRevenueDashboardHandler;

namespace Infrastructure.Persistence.Repositories
{
    public class WalletTransactionRepository(AppDbContext context) : IWalletTransactionRepository
    {
        public async Task AddAsync(WalletTransaction transaction)

            => await context.WalletTransactions.AddAsync(transaction);

        public async Task<PagenatedList<WalletTransaction>> GetAllAsync(PageRequest request, bool usePaging)
        {
            var query = context.WalletTransactions.Where(x => !x.IsDeleted).AsQueryable();
            var totalCount = await query.CountAsync();

            if (usePaging)
            {
                int currentPage = request.Page < 1 ? 1 : request.Page;
                int pageSize = request.PageSize < 1 ? 10 : request.PageSize;

                var set = query
                    .Include(x => x.Wallet)
                    .ThenInclude(x => x.User)
                    .Skip((currentPage - 1) * pageSize)
                    .Take(pageSize);

                return new PagenatedList<WalletTransaction>
                {
                    Items = await set.ToListAsync(),
                    Page = currentPage,
                    PageSize = pageSize,
                    TotalCount = totalCount
                };
            }

            return new PagenatedList<WalletTransaction>
            {
                Items = await query.Include(x => x.Wallet).ToListAsync(),
                TotalCount = totalCount
            };
        }

        public async Task<WalletTransaction?> GetByIdAsync(Guid id)

            => await context.WalletTransactions
                .FirstOrDefaultAsync(t => t.Id == id && !t.IsDeleted);

        public Task<WalletTransaction?> GetByReferenceAsync(string reference)

            => context.WalletTransactions
                .FirstOrDefaultAsync(t => t.PaystackReference == reference && !t.IsDeleted);

        public async Task<PagenatedList<WalletTransaction>> GetByTransactionStatusAsync(Guid walletId, WalletTransactionStatus status, PageRequest request, bool usePaging)
        {
            var query = context.WalletTransactions
                .Where(x => !x.IsDeleted && x.WalletId == walletId && x.Status == status)
                .AsQueryable();

            var totalCount = await query.CountAsync();

            if (usePaging)
            {
                int currentPage = request.Page < 1 ? 1 : request.Page;
                int pageSize = request.PageSize < 1 ? 10 : request.PageSize;

                var set = query
                    .Include(x => x.Wallet)
                    .ThenInclude(x => x.User)
                    .Skip((currentPage - 1) * pageSize)
                    .Take(pageSize);

                return new PagenatedList<WalletTransaction>
                {
                    Items = await set.ToListAsync(),
                    Page = currentPage,
                    PageSize = pageSize,
                    TotalCount = totalCount
                };
            }

            return new PagenatedList<WalletTransaction>
            {
                Items = await query.Include(x => x.Wallet).ToListAsync(),
                TotalCount = totalCount
            };
        }

        public async Task<PagenatedList<WalletTransaction>> GetByTransactionStatusAsync(WalletTransactionStatus status, PageRequest request, bool usePaging)
        {
            var query = context.WalletTransactions
                .Where(x => !x.IsDeleted && x.Status == status )
                .AsQueryable();
            var totalCount = await query.CountAsync();

            if (usePaging)
            {
                int currentPage = request.Page < 1 ? 1 : request.Page;
                int pageSize = request.PageSize < 1 ? 10 : request.PageSize;

                var set = query
                    .Include(x => x.Wallet)
                    .ThenInclude(x => x.User)
                    .Skip((currentPage - 1) * pageSize)
                    .Take(pageSize);

                return new PagenatedList<WalletTransaction>
                {
                    Items = await set.ToListAsync(),
                    Page = currentPage,
                    PageSize = pageSize,
                    TotalCount = totalCount
                };
            }

            return new PagenatedList<WalletTransaction>
            {
                Items = await query.Include(x => x.Wallet).ToListAsync(),
                TotalCount = totalCount
            };
        }

        public async Task<PagenatedList<WalletTransaction>> GetByTransactionTypeAsync(Guid walletId, TransactionType type, PageRequest request, bool usePaging)
        {
            var query = context.WalletTransactions
                .Where(x => !x.IsDeleted && x.WalletId == walletId && x.Type == type)
                .AsQueryable();

            var totalCount = await query.CountAsync();

            if (usePaging)
            {
                int currentPage = request.Page < 1 ? 1 : request.Page;
                int pageSize = request.PageSize < 1 ? 10 : request.PageSize;

                var set = query
                    .Include(x => x.Wallet)
                    .ThenInclude(x => x.User)
                    .Skip((currentPage - 1) * pageSize)
                    .Take(pageSize);

                return new PagenatedList<WalletTransaction>
                {
                    Items = await set.ToListAsync(),
                    Page = currentPage,
                    PageSize = pageSize,
                    TotalCount = totalCount
                };
            }

            return new PagenatedList<WalletTransaction>
            {
                Items = await query.Include(x => x.Wallet).ToListAsync(),
                TotalCount = totalCount
            };
        }

        public async Task<PagenatedList<WalletTransaction>> GetByTransactionTypeAsync(TransactionType type, PageRequest request, bool usePaging)
        {
            var query = context.WalletTransactions
                .Where(x => !x.IsDeleted && x.Type == type)
                .AsQueryable();

            var totalCount = await query.CountAsync();

            if (usePaging)
            {
                int currentPage = request.Page < 1 ? 1 : request.Page;
                int pageSize = request.PageSize < 1 ? 10 : request.PageSize;

                var set = query
                    .Include(x => x.Wallet)
                    .ThenInclude(x => x.User)
                    .Skip((currentPage - 1) * pageSize)
                    .Take(pageSize);

                return new PagenatedList<WalletTransaction>
                {
                    Items = await set.ToListAsync(),
                    Page = currentPage,
                    PageSize = pageSize,
                    TotalCount = totalCount
                };
            }

            return new PagenatedList<WalletTransaction>
            {
                Items = await query.Include(x => x.Wallet).ToListAsync(),
                TotalCount = totalCount
            };
        }

        public async Task<PagenatedList<WalletTransaction>> GetByWalletIdAsync(Guid walletId, PageRequest request, bool usePaging)
        {
            var query = context.WalletTransactions
                .Where(x => !x.IsDeleted && x.WalletId == walletId)
                .AsQueryable();

            var totalCount = await query.CountAsync();

            if (usePaging)
            {
                int currentPage = request.Page < 1 ? 1 : request.Page;
                int pageSize = request.PageSize < 1 ? 10 : request.PageSize;

                var set = query
                    .Include(x => x.Wallet)
                    .ThenInclude(x => x.User)
                    .Skip((currentPage - 1) * pageSize)
                    .Take(pageSize);

                return new PagenatedList<WalletTransaction>
                {
                    Items = await set.ToListAsync(),
                    Page = currentPage,
                    PageSize = pageSize,
                    TotalCount = totalCount
                };
            }

            return new PagenatedList<WalletTransaction>
            {
                Items = await query.Include(x => x.Wallet).ToListAsync(),
                TotalCount = totalCount
            };
        }

        public async Task<decimal> GetSumByDateRangeAsync(
        Guid walletId,
        DateTime startDate,
        DateTime endDate,
        CancellationToken cancellationToken = default)
        {
            // Include the full end day (e.g. May 31 23:59:59.999)
            var inclusiveEnd = endDate.Date.AddDays(1).AddTicks(-1);

            return await context.WalletTransactions
                .Where(t => t.WalletId == walletId
                            && t.DateCreated >= startDate.Date
                            && t.DateCreated <= inclusiveEnd
                            && t.Type == TransactionType.Credit
                            && t.Status == WalletTransactionStatus.Successful)
                .SumAsync(t => (decimal?)t.Balance, cancellationToken) ?? 0m;
        }

        public async Task<decimal> GetPendingPayoutAsync(Guid walletId)
        {
            var wallet = await context.Wallets.FirstOrDefaultAsync(w => w.Id == walletId);
            var since = wallet?.LastPayoutDate;

            return await context.WalletTransactions
                .Where(t => t.WalletId == walletId
                         && t.Type == TransactionType.Credit
                         && (since == null || t.DateCreated > since))
                .SumAsync(t => t.Balance);
        }

        public async Task<decimal> GetPendingPayoutAmountAsync(Guid walletId)
        {
            return await context.WalletTransactions
                .Where(t => t.WalletId == walletId
                         && t.Type == TransactionType.Payout
                         && t.Status == WalletTransactionStatus.Pending)
                .SumAsync(t => t.Balance);
        }

        public async Task<int> GetUnpaidMonthsCountAsync(Guid walletId)
        {
            return await context.WalletTransactions
                .CountAsync(t => t.WalletId == walletId
                               && t.Type == TransactionType.Payout
                               && t.Status == WalletTransactionStatus.Pending);
        }

        public async Task<WalletTransaction?> GetLastPaidPayoutAsync(Guid walletId)
        {
            return await context.WalletTransactions
                .Where(t => t.WalletId == walletId
                         && t.Type == TransactionType.Payout
                         && t.Status == WalletTransactionStatus.Successful)
                .OrderByDescending(t => t.DateCreated)
                .FirstOrDefaultAsync();
        }

        public async Task<PayoutStatusOverviewDto> GetPayoutStatusOverviewAsync(Guid walletId)
        {
            var payouts = await context.WalletTransactions
                .Where(t => t.WalletId == walletId && t.Type == TransactionType.Payout)
                .ToListAsync();

            var paid = payouts.Where(p => p.Status == WalletTransactionStatus.Successful).ToList();
            var pending = payouts.Where(p => p.Status == WalletTransactionStatus.Pending).ToList();

            return new PayoutStatusOverviewDto(
                PaidMonthsCount: paid.Count,
                PendingMonthsCount: pending.Count,
                TotalPaidAmount: paid.Sum(p => p.Balance),
                TotalPendingAmount: pending.Sum(p => p.Balance)
            );
        }
    }
}
