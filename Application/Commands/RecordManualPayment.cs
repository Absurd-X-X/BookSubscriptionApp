using Application.Common.Dtos;
using Application.Common.Repositories;
using Domain.Entities;
using Domain.Enums;
using MediatR;

namespace Application.Commands
{
    public class RecordManualPayment
    {
        public record RecordManualPaymentCommand(
            Guid UserId,
            Guid RecipientId,
            decimal Amount) : IRequest<Result<RecordManualPaymentResponse>>;

        public class RecordManualPaymentHandler(
            IUserRepository userRepository,
            IWalletRepository walletRepository,
            IWalletTransactionRepository walletTransactionRepository,
            IUnitOfWork unitOfWork) : IRequestHandler<RecordManualPaymentCommand, Result<RecordManualPaymentResponse>>
{
            public async Task<Result<RecordManualPaymentResponse>> Handle(RecordManualPaymentCommand request, CancellationToken cancellationToken)
            {
                var user = await userRepository
                    .GetAsync(request.UserId);
                if (user is null)
                    return Result<RecordManualPaymentResponse>.Failure("Customer not found");


                var recipient = await userRepository
                    .GetAsync(request.RecipientId);
                if (recipient is null)
                    return Result<RecordManualPaymentResponse>.Failure("Recipient not found");

                var wallet = await walletRepository
                    .GetByUserIdAsync(request.UserId);
                if (wallet is null)
                    return Result<RecordManualPaymentResponse>.Failure("Wallet not found");


                var recipientwallet = await walletRepository
                    .GetByUserIdAsync(request.RecipientId);
                if (recipientwallet is null)
                    return Result<RecordManualPaymentResponse>.Failure("Recipient Wallet not found");

                if (request.Amount <= 0)
                {
                    return Result<RecordManualPaymentResponse>
                        .Failure("Amount must be greater than zero.");
                }

                if (request.UserId == request.RecipientId)
                {
                    return Result<RecordManualPaymentResponse>.Failure(
                        "Sender and recipient cannot be the same."
                    );
                }

                if (user.Role.ToLower() != "admin")
                {
                    return Result<RecordManualPaymentResponse>
                        .Failure("Only Super Admin can record manual payments.");
                }

                if (recipient.Role.ToLower() != "library")
                {
                    return Result<RecordManualPaymentResponse>
                        .Failure("Recipient must be a Library Admin.");
                }

                if (wallet.Balance < request.Amount)
                    return Result<RecordManualPaymentResponse>.Failure("Sorry! the transaction can't be completed due to insufficient wallet bal.");

                decimal systemWalletBefore = wallet.Balance;
                decimal recipientBalBefore = recipientwallet.Balance;

                wallet.Balance -= request.Amount;
                recipientwallet.Balance += request.Amount;

                var reference = $"MANPAY-{Guid.NewGuid():N}";

                await walletTransactionRepository.AddAsync(new WalletTransaction
                {
                    WalletId = wallet.Id,
                    Balance = request.Amount,
                    Type = TransactionType.Debit,
                    Status = WalletTransactionStatus.Successful,
                    PaystackReference = reference,
                    Description = "Monthly library payout",
                    BalanceBefore = systemWalletBefore,
                    BalanceAfter = wallet.Balance,
                    CreatedBy = user.Id.ToString()

                });

                await walletTransactionRepository.AddAsync(new WalletTransaction
                {
                    WalletId = recipientwallet.Id,
                    Balance = request.Amount,
                    Type = TransactionType.Credit,
                    Status = WalletTransactionStatus.Successful,
                    PaystackReference = reference,
                    Description = "Monthly payment received",
                    BalanceBefore = recipientBalBefore,
                    BalanceAfter = recipientwallet.Balance,
                    CreatedBy = user.Id.ToString()
                });

                await unitOfWork.SaveAsync();

                return Result<RecordManualPaymentResponse>.Success(new RecordManualPaymentResponse(reference),
                    "Recorded Manual Payment successfully"
                    );
            }
        }

        public record RecordManualPaymentResponse(string Reference
            );
    }
}
