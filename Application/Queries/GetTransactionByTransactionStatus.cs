using Application.Common.Dtos;
using Application.Common.Pagenation;
using Application.Common.Repositories;
using Domain.Enums;
using MediatR;

namespace Application.Queries
{
    public class GetTransactionByTransactionStatus
    {
        public record GetTransactionByTransactionStatusCommand(int Page, int PageSize, WalletTransactionStatus Status) :
            IRequest<Result<PagenatedList<GetTransactionByTransactionStatusResponse>>>;

        public class GetTransactionByTransactionStatusHandler(
            IWalletTransactionRepository walletTransactionRepository) :
            IRequestHandler<GetTransactionByTransactionStatusCommand, Result<PagenatedList<GetTransactionByTransactionStatusResponse>>>
        {
            public async Task<Result<PagenatedList<GetTransactionByTransactionStatusResponse>>> Handle(GetTransactionByTransactionStatusCommand request, CancellationToken cancellationToken)
            {

                var transactions = await walletTransactionRepository
                    .GetByTransactionStatusAsync(request.Status, new PageRequest { Page = request.Page, PageSize = request.PageSize }, true);

                var response = transactions.Items.Select(t =>
                    new GetTransactionByTransactionStatusResponse(
                        t.Id, t.Balance, t.Type.ToString(),
                        t.Status.ToString(), t.Description,
                        t.BalanceBefore, t.BalanceAfter,
                        t.DateCreated))
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
        Guid TransactionId, decimal Amount,
        string Type, string Status, string Description,
        decimal BalanceBefore, decimal BalanceAfter,
        DateTime DateCreated);
}
