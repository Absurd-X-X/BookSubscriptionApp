using Application.Common.Pagenation;
using Domain.Entities;
using Domain.Enums;
using Org.BouncyCastle.Asn1.Ocsp;
using System.Transactions;

namespace Application.Common.Repositories
{
    public interface IWalletTransactionRepository
    {
        Task AddAsync(WalletTransaction transaction);
        Task<WalletTransaction?> GetByIdAsync(Guid id);
        Task<WalletTransaction?> GetByReferenceAsync(string reference);
        Task<PagenatedList<WalletTransaction>> GetByWalletIdAsync(Guid walletId, PageRequest request, bool usepaging);
        Task<PagenatedList<WalletTransaction>> GetByTransactionTypeAsync(Guid walletId, TransactionType type, PageRequest request, bool usepaging);
        Task<PagenatedList<WalletTransaction>> GetByTransactionStatusAsync(Guid walletId, WalletTransactionStatus status, PageRequest request, bool usepaging);
        Task<PagenatedList<WalletTransaction>> GetAllAsync(PageRequest request, bool usepaging);
        Task<PagenatedList<WalletTransaction>> GetByTransactionTypeAsync(TransactionType type, PageRequest request, bool usepaging);
        Task<PagenatedList<WalletTransaction>> GetByTransactionStatusAsync(WalletTransactionStatus status, PageRequest request, bool usepaging);
        Task<decimal> GetSumByDateRangeAsync(
        Guid walletId,
        DateTime startDate,
        DateTime endDate,
        CancellationToken cancellationToken = default);
        Task<decimal> GetPendingPayoutAmountAsync(Guid walletId);
        Task<int> GetUnpaidMonthsCountAsync(Guid walletId);
        Task<WalletTransaction?> GetLastPaidPayoutAsync(Guid walletId);
    }
}
