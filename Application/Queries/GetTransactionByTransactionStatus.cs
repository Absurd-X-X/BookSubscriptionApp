using Application.Common.Dtos;
using Application.Common.Pagenation;
using Application.Common.Repositories;
using Domain.Entities;
using Domain.Enums;
using MediatR;

namespace Application.Queries
{
    public class GetTransactionByTransactionStatus
    {
        public record GetTransactionByTypeQuery(int Page, int PageSize, TransactionType Type) :
            IRequest<Result<PagenatedList<GetTransactionByTransactionStatusResponse>>>;

        public class GetTransactionByTransactionStatusHandler(
            IWalletTransactionRepository walletTransactionRepository) :
            IRequestHandler<GetTransactionByTypeQuery, Result<PagenatedList<GetTransactionByTransactionStatusResponse>>>
        {
            public async Task<Result<PagenatedList<GetTransactionByTransactionStatusResponse>>> Handle(GetTransactionByTypeQuery request, CancellationToken cancellationToken)
            {

                var transactions = await walletTransactionRepository
                    .GetByTransactionTypeAsync(request.Type, new PageRequest { Page = request.Page, PageSize = request.PageSize }, true);

                var response = transactions.Items.Select(transaction =>
                    new GetTransactionByTransactionStatusResponse(
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

                var pagenated = new PagenatedList<GetTransactionByTransactionStatusResponse>
                {
                    Items = response,
                    Page = request.Page,
                    PageSize = request.PageSize,
                    TotalCount = response.Count,
                };

                return Result<PagenatedList<GetTransactionByTransactionStatusResponse>>
                    .Success(pagenated, "Success");
            }
        }
    }

    public record GetTransactionByTransactionStatusResponse(

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
