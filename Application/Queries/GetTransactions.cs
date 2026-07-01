using Application.Common.Dtos;
using Application.Common.Pagenation;
using Application.Common.Repositories;
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

                var response = transactions.Items.Select(t =>
                    new GetTransactionsResponse(
                        t.Id, t.Balance, t.Type.ToString(),
                        t.Status.ToString(), t.Description,
                        t.BalanceBefore, t.BalanceAfter,
                        t.DateCreated))
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
            Guid TransactionId, decimal Amount,
            string Type, string Status, string Description,
            decimal BalanceBefore, decimal BalanceAfter,
            DateTime DateCreated);
    }
}