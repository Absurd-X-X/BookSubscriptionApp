using Application.Common.Pagenation;
using Application.Common.Repositories;
using Domain.Entities;
using Domain.Enums;
using Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;

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
    }
}
