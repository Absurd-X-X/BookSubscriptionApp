using Application.Common;
using Application.Common.Dtos;
using Application.Common.Repositories;
using Application.Services;
using Domain.Entities;
using Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace Application.Commands
{
    public class VerifyFunding
    {
        public record VerifyFundingCommand(
            string Reference) : IRequest<Result<string>>;


        public class VerifyFundingHandler(
            IWalletRepository walletRepository,
            IWalletTransactionRepository walletTransactionRepository,
            IPaystackService paystackService,
            IEmailService emailService,
            IHttpContextAccessor httpContextAccessor,
            IAuditLogRepository auditLogRepository,
            IUnitOfWork unitOfWork)
            : IRequestHandler<VerifyFundingCommand, Result<string>>
        {
            public async Task<Result<string>> Handle(
                VerifyFundingCommand request,
                CancellationToken cancellationToken)
            {
                var transaction = await walletTransactionRepository
                    .GetByReferenceAsync(request.Reference);

                if (transaction is null)
                    return Result<string>.Failure("Transaction not found");

                if (transaction.Status == WalletTransactionStatus.Successful)
                    return Result<string>.Failure("Transaction already verified");

                var verification = await paystackService
                    .VerifyPaymentAsync(request.Reference);

                if (!verification.Status || verification.Data.PaymentStatus != "success")
                {
                    transaction.Status = WalletTransactionStatus.Failed;
                    transaction.DateModified = DateTime.UtcNow;
                    await unitOfWork.SaveAsync();
                    return Result<string>.Failure("Payment verification failed");
                }

                if (verification.Data.Amount != transaction.Balance)
                {
                    transaction.Status = WalletTransactionStatus.Failed;
                    transaction.DateModified = DateTime.UtcNow;
                    await unitOfWork.SaveAsync();
                    return Result<string>.Failure("Payment amount mismatch");
                }

                var wallet = await walletRepository
                    .GetAsync(transaction.WalletId);

                transaction.BalanceBefore = wallet!.Balance;
                wallet.Balance += transaction.Balance;
                transaction.BalanceAfter = wallet.Balance;
                transaction.Status = WalletTransactionStatus.Successful;
                transaction.DateModified = DateTime.UtcNow;

                await unitOfWork.SaveAsync();

                var emailResult = await emailService.SendEmailAsync(
                    wallet.User.Email,
                    "Wallet Funded Successfully",
                    EmailTemplates.WalletFundedEmail(
                        wallet.User.UserName,
                        transaction.Balance,
                        wallet.Balance));

                string? ipAddress = httpContextAccessor
                .HttpContext?
                .Connection
                .RemoteIpAddress?
                .ToString();


                var audit = new AuditLog
                {
                    ActionType = "Verify",
                    Description = $"You verified your funding",
                    Icon = "🏷️",
                    IpAddress = ipAddress!,
                    UserRole = wallet.User.Role,
                    UserId = wallet.User.Id,
                    ResourceType = ResourceType.System,
                    ResourceId = null,
                };

                await auditLogRepository.AddAsync(audit);
                await unitOfWork.SaveAsync();

                if (!emailResult.Success)
                {
                    // Wallet was funded successfully; email is secondary, so we still return success
                    // but flag it so the caller/logs can surface a soft warning if needed
                    return Result<string>.Success(
                        "Wallet funded successfully! (Confirmation email could not be sent)", "funded");
                }

                return Result<string>.Success(
                    "Wallet funded successfully!", "funded");
            }
        }
    }
}