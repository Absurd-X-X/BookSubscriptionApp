using Application.Common.Dtos;
using Application.Common.Pagenation;
using Application.Common.Repositories;
using MediatR;

namespace Application.Queries
{
    public class GetTransactionHistory
    {
        public record GetTransactionHistoryQuery(Guid UserId, int Page, int PageSize)
            : IRequest<Result<PagenatedList<GetTransactionHistoryResponse>>>;

        public class GetTransactionHistoryHandler(
            IWalletRepository walletRepository,
            IWalletTransactionRepository walletTransactionRepository)
            : IRequestHandler<GetTransactionHistoryQuery,
                Result<PagenatedList<GetTransactionHistoryResponse>>>
        {
            public async Task<Result<PagenatedList<GetTransactionHistoryResponse>>> Handle(
                GetTransactionHistoryQuery request,
                CancellationToken cancellationToken)
            {
                var wallet = await walletRepository
                    .GetByUserIdAsync(request.UserId);

                if (wallet is null)
                    return Result<PagenatedList<GetTransactionHistoryResponse>>
                        .Failure("Wallet not found");

                var transactions = await walletTransactionRepository
                    .GetByWalletIdAsync(wallet.Id, new PageRequest {Page = request.Page, PageSize = request.PageSize}, true);

                var response = transactions.Items.Select(t =>
                    new GetTransactionHistoryResponse(
                        t.Id, t.Balance, t.Type.ToString(),
                        t.Status.ToString(), t.Description,
                        t.BalanceBefore, t.BalanceAfter,
                        t.DateCreated))
                    .ToList();

                var pagenated = new PagenatedList<GetTransactionHistoryResponse>
                {
                    Items = response,
                    Page = request.Page,
                    PageSize = request.PageSize,
                    TotalCount = transactions.TotalCount,
                };

                return Result<PagenatedList<GetTransactionHistoryResponse>>
                    .Success(pagenated, "Success");
            }
        }

        public record GetTransactionHistoryResponse(
            Guid TransactionId, decimal Amount,
            string Type, string Status, string Description,
            decimal BalanceBefore, decimal BalanceAfter,
            DateTime DateCreated);
    }
}