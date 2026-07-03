using Application.Common.Dtos;
using Application.Common.Pagenation;
using Application.Common.Repositories;
using Domain.Entities;
using Domain.Enums;
using MediatR;

namespace Application.Queries
{
    public class GetPendingTransaction
    {
        public record GetPendingTransactionQuery(int Page, int PageSize, WalletTransactionStatus Status) : IRequest<Result<PagenatedList<GetPendingTransactionResponse>>>;

        public class GetPendingTransactionHandler(
            IWalletTransactionRepository walletTransactionRepository) : IRequestHandler<GetPendingTransactionQuery, Result<PagenatedList<GetPendingTransactionResponse>>>
        {
            public async Task<Result<PagenatedList<GetPendingTransactionResponse>>> Handle(GetPendingTransactionQuery request, CancellationToken cancellationToken)
            {
                var transactions = await walletTransactionRepository
                    .GetByTransactionStatusAsync(request.Status, new PageRequest { Page = request.Page, PageSize = request.PageSize }, true);

                var response = transactions.Items.Select(transaction =>
                    new GetPendingTransactionResponse(
                    transaction.Id,
                    transaction.WalletId,
                    transaction.Balance,
                    transaction.Type,
                    transaction.Status,
                    transaction.PaystackReference,
                    transaction.Description,
                    transaction.BalanceBefore,
                    transaction.BalanceAfter,
                    transaction.CreatedBy,
                    transaction.DateCreated,
                    transaction.Wallet))
                   .ToList();

                var pagenated = new PagenatedList<GetPendingTransactionResponse>
                {
                    Items = response,
                    Page = request.Page,
                    PageSize = request.PageSize,
                    TotalCount = response.Count,
                };

                return Result<PagenatedList<GetPendingTransactionResponse>>
                    .Success(pagenated, "Success");
            }
        }
    }

    public record GetPendingTransactionResponse(
            Guid Id,
            Guid WalletId,
            decimal Balance,
            TransactionType Type,
            WalletTransactionStatus Status,
            string? PaystackReference,
            string Description,
            decimal BalanceBefore,
            decimal BalanceAfter,
            string CreatedBy,
            DateTime DateCreated,
            Wallet Wallet
        );
}
