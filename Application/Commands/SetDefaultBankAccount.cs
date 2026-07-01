using Application.Common.Dtos;
using Application.Common.Repositories;
using MediatR;

namespace Application.Commands
{
    public class SetDefaultBankAccount
    {
        public record SetDefaultBankAccountCommand(
            Guid BankAccountId,
            Guid UserId) : IRequest<Result<string>>;

        public class SetDefaultBankAccountHandler(
            IBankAccountRepository bankAccountRepository,
            IUnitOfWork unitOfWork)
            : IRequestHandler<SetDefaultBankAccountCommand, Result<string>>
        {
            public async Task<Result<string>> Handle(
                SetDefaultBankAccountCommand request,
                CancellationToken cancellationToken)
            {
                var accounts = await bankAccountRepository
                    .GetAllAccountByUserAsync(request.UserId);

                var target = accounts
                    .FirstOrDefault(a => a.Id == request.BankAccountId);

                if (target is null)
                    return Result<string>.Failure("Bank account not found");

                foreach (var account in accounts.Where(a => a.IsDefault))
                {
                    account.IsDefault = false;
                    account.DateModified = DateTime.UtcNow;
                }

                target.IsDefault = true;
                target.DateModified = DateTime.UtcNow;

                await unitOfWork.SaveAsync();

                return Result<string>.Success(
                    "Default bank account updated!", "updated");
            }
        }
    }
}