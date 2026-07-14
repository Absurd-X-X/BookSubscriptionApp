using Application.Common.Dtos;
using Application.Common.Repositories;
using Domain.Enums;
using MediatR;

namespace Application.Queries
{
    public class GetWalletDashboard
    {
        public record GetWalletDashboardQuery(
            Guid ReaderId
            ) : IRequest<Result<GetWalletDashboardResponse>>;

        public class GetWalletDashboardHandler(
            IWalletRepository walletRepository
            ) : IRequestHandler<GetWalletDashboardQuery, Result<GetWalletDashboardResponse>>
        {
            async Task<Result<GetWalletDashboardResponse>> IRequestHandler<GetWalletDashboardQuery, Result<GetWalletDashboardResponse>>.
                Handle(GetWalletDashboardQuery request, CancellationToken cancellationToken)
            {
                var wallet = await walletRepository.GetByUserIdAsync(request.ReaderId);
                if (wallet is null)
                    return Result<GetWalletDashboardResponse>.Failure("Wallet not found");

                var activeTransactions = wallet.Transactions.Where(t => !t.IsDeleted).ToList();

                var pendingFunds = activeTransactions
                    .Where(t => t.Type == TransactionType.Credit && t.Status == WalletTransactionStatus.Pending)
                    .Sum(t => t.Balance);

                var totalSpentThisYear = activeTransactions
                    .Where(t => t.Type == TransactionType.Debit
                             && t.Status == WalletTransactionStatus.Successful
                             && t.DateCreated.Year == DateTime.UtcNow.Year)
                    .Sum(t => t.Balance);

                var recentTransactions = activeTransactions
                    .OrderByDescending(t => t.DateCreated)
                    .Take(5)
                    .Select(t => new WalletTransactionResponse(
                        t.Id,
                        t.DateCreated,
                        t.Type,
                        t.Description,
                        t.Balance,
                        t.Status,
                        t.PaystackReference
                        ))
                    .ToList();

                var data = new GetWalletDashboardResponse(
                    wallet.Balance,
                    pendingFunds,
                    totalSpentThisYear,
                    recentTransactions
                    );

                return Result<GetWalletDashboardResponse>.Success(data, "Retrieved");
            }
        }

        public record GetWalletDashboardResponse(
            decimal Balance,
            decimal PendingFunds,
            decimal TotalSpentThisYear,
            List<WalletTransactionResponse> RecentTransactions
            );

        public record WalletTransactionResponse(
            Guid Id,
            DateTime Date,
            TransactionType Type,
            string Description,
            decimal Amount,
            WalletTransactionStatus Status,
            string? Reference
            );
    }
}