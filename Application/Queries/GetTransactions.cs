using Application.Common.Dtos;
using Application.Common.Pagenation;
using Application.Common.Repositories;
using Domain.Entities;
using Domain.Enums;
using MediatR;

namespace Application.Queries
{
    public class GetTransactions
    {
        public record GetTransactionsQuery(int Page, int PageSize)
            : IRequest<Result<PagenatedList<GetTransactionsResponse>>>;

        public class GetTransactionsHandler(
            IWalletTransactionRepository walletTransactionRepository)
            : IRequestHandler<GetTransactionsQuery,
                Result<PagenatedList<GetTransactionsResponse>>>
        {
            public async Task<Result<PagenatedList<GetTransactionsResponse>>> Handle(
                GetTransactionsQuery request,
                CancellationToken cancellationToken)
            {

                var transactions = await walletTransactionRepository
                    .GetAllAsync(new PageRequest
                    {
                        Page = request.Page,
                        PageSize = request.PageSize
                    }, true);

                var response = transactions.Items.Select(transaction =>
                    new GetTransactionsResponse(transaction.Id,
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

                var transactionsResponse = new PagenatedList<GetTransactionsResponse>
                {
                    Items = response,
                    TotalCount = response.Count,
                    Page = 1,
                    PageSize = response.Count
                };

                return Result<PagenatedList<GetTransactionsResponse>>
                    .Success(transactionsResponse, "Success");
            }
        }

        public record GetTransactionsResponse(
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
            Wallet Wallet);
    }
}