using Application.Common.Dtos;
using Application.Common.Repositories;
using Application.Services;
using Domain.Entities;
using Domain.Enums;
using MediatR;

namespace Application.Commands
{
    public class FundWallet
    {
        public record FundWalletCommand(
            Guid UserId,
            decimal Amount) : IRequest<Result<FundWalletResponse>>;

        public class FundWalletHandler(
            IUserRepository userRepository,
            IWalletRepository walletRepository,
            IWalletTransactionRepository walletTransactionRepository,
            IPaystackService paystackService,
            IUnitOfWork unitOfWork)
            : IRequestHandler<FundWalletCommand, Result<FundWalletResponse>>
        {
            public async Task<Result<FundWalletResponse>> Handle(
                FundWalletCommand request,
                CancellationToken cancellationToken)
            {
                var customer = await userRepository
                    .GetAsync(request.UserId);
                if (customer is null)
                    return Result<FundWalletResponse>.Failure("Customer not found");

                var wallet = await walletRepository
                    .GetByUserIdAsync(request.UserId);
                if (wallet is null)
                    return Result<FundWalletResponse>.Failure("Wallet not found");

                var reference = $"FUND-{Guid.NewGuid():N}";

                var paystackResponse = await paystackService.InitializePaymentAsync(
                    customer.Email, request.Amount, reference);

                if (!paystackResponse.Status)
                    return Result<FundWalletResponse>.Failure(
                        "Could not initialize payment");

                wallet.Balance += request.Amount;

                await walletTransactionRepository.AddAsync(new WalletTransaction
                {
                    WalletId = wallet.Id,
                    Balance = request.Amount,
                    Type = TransactionType.Credit,
                    Status = WalletTransactionStatus.Pending,
                    PaystackReference = reference,
                    Description = "Wallet Funding",
                    BalanceBefore = wallet.Balance,
                    BalanceAfter = wallet.Balance += request.Amount,
                    CreatedBy = customer.Email
                });

                await unitOfWork.SaveAsync();

                return Result<FundWalletResponse>.Success(new FundWalletResponse(
                        paystackResponse.AuthorizationUrl, reference),
                    "Payment initialized"
                    );
            }
        }

        public record FundWalletResponse(
            string AuthorizationUrl, string Reference);
    }
}