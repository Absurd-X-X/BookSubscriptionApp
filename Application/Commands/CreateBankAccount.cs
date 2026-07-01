using Application.Common.Dtos;
using Application.Common.Repositories;
using Application.Services;
using Domain.Entities;
using MediatR;

namespace Application.Commands
{
    public class AddBankAccount
    {
        public record AddBankAccountCommand(
            Guid UserId,
            string BankName,
            string BankCode,
            string AccountNumber,
            string AccountName,
            bool IsDefault) : IRequest<Result<string>>;

        public class AddBankAccountHandler(
            IBankAccountRepository bankAccountRepository,
            IPaystackService paystackService,
            IUnitOfWork unitOfWork,
            IUserRepository userRepository
            )
            : IRequestHandler<AddBankAccountCommand, Result<string>>
        {
            public async Task<Result<string>> Handle(AddBankAccountCommand request, CancellationToken cancellationToken)
            {
                var verification = await paystackService.VerifyAccountNumberAsync(request.AccountNumber, request.BankCode);

                if (!verification.Status) return Result<string>.Failure("Account verification failed");

                var recipientCode = await paystackService.CreateTransferRecipientAsync(request.AccountName, request.AccountNumber, request.BankCode);

                var user = await userRepository.GetAsync(request.UserId);

                if (user == null)
                    return Result<string>.Failure("User not found");

                if (request.IsDefault)
                {
                    var existing = await bankAccountRepository.GetAllAccountByUserAsync(user.Id);

                    foreach (var account in existing.Where(a => a.IsDefault))
                    {
                        account.IsDefault = false;
                        account.DateModified = DateTime.UtcNow;
                    }
                }

                await bankAccountRepository.AddAsync(new BankAccount
                {
                    UserId = user.Id,
                    BankName = request.BankName,
                    BankCode = request.BankCode,
                    AccountNumber = request.AccountNumber,
                    AccountName = request.AccountName,
                    RecipientCode = recipientCode,
                    IsDefault = request.IsDefault,
                    CreatedBy = user.Id.ToString()
                });

                await unitOfWork.SaveAsync();

                return Result<string>.Success("Bank account added successfully", "added");
            }
        }
    }
}