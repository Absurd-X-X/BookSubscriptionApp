using Application.Common.Dtos;
using Application.Common.Pagenation;
using Application.Common.Repositories;
using Domain.Enums;
using MediatR;

namespace Application.Queries
{
    public class GetPendingTransaction
    {
        public record GetPendingTransactionQuery(int Page, int PageSize, TransactionType Type) : IRequest<Result<PagenatedList<GetPendingTransactionResponse>>>;

        public class GetPendingTransactionHandler(
            IWalletTransactionRepository walletTransactionRepository) : IRequestHandler<GetPendingTransactionQuery, Result<PagenatedList<GetPendingTransactionResponse>>>
        {
            public async Task<Result<PagenatedList<GetPendingTransactionResponse>>> Handle(GetPendingTransactionQuery request, CancellationToken cancellationToken)
            {
                var transactions = await walletTransactionRepository
                    .GetByTransactionTypeAsync(request.Type, new PageRequest { Page = request.Page, PageSize = request.PageSize }, true);

                var response = transactions.Items.Select(t =>
                    new GetPendingTransactionResponse(
                        t.Id, t.Balance, t.Type.ToString(),
                        t.Status.ToString(), t.Description,
                        t.BalanceBefore, t.BalanceAfter,
                        t.DateCreated))
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
        Guid TransactionId, decimal Amount,
        string Type, string Status, string Description,
        decimal BalanceBefore, decimal BalanceAfter,
        DateTime DateCreated);

}
