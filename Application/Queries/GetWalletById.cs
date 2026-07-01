using Application.Common.Dtos;
using Application.Common.Repositories;
using MediatR;

namespace Application.Queries
{
    public class GetWalletBalance
    {
        public record GetWalletBalanceQuery(Guid UserId)
            : IRequest<Result<GetWalletBalanceResponse>>;

        public class GetWalletBalanceHandler(IWalletRepository walletRepository)
            : IRequestHandler<GetWalletBalanceQuery,
                Result<GetWalletBalanceResponse>>
        {
            public async Task<Result<GetWalletBalanceResponse>> Handle(
                GetWalletBalanceQuery request,
                CancellationToken cancellationToken)
            {
                var wallet = await walletRepository
                    .GetByUserIdAsync(request.UserId);
                if (wallet is null)
                    return Result<GetWalletBalanceResponse>.Failure("Wallet not found");

                return Result<GetWalletBalanceResponse>.Success(
                    
                    new GetWalletBalanceResponse(wallet.Id, wallet.Balance), "Success");
            }
        }

        public record GetWalletBalanceResponse(Guid WalletId, decimal Balance);
    }
}