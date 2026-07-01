using Application.Common.Dtos;
using Application.Common.Repositories;
using MediatR;

namespace Application.Command
{
    public class UpdateBankAccount
    {
        public record UpdateAcountCommand(
            Guid AccountId,
            string AccountName,
            string AccountNumber,
            string BankName
            ) : IRequest<Result<string>>;

        public class UpdateAccountHandler(
            IBankAccountRepository bankAccountRepository,
            IUnitOfWork unitOfWork,
            ICurrentUser currentUser,
            IUserRepository userRepository
            ) : IRequestHandler<UpdateAcountCommand, Result<string>>
        {
            public async Task<Result<string>> Handle(UpdateAcountCommand request, CancellationToken cancellationToken)
            {
                var account = await bankAccountRepository.GetByIdAsync( request.AccountId );
                var userId = currentUser.GetCurrentUser();
                var user = await userRepository.GetAsync(userId);

                if ( user == null)
                    return Result<string>.Failure("User not found");


                if (account == null)
                    return Result<string>.Failure("Account not found");

                if (user.Id != account.UserId)
                    return Result<string>.Failure("Unauthirized");

                account.AccountNumber = request.AccountNumber;
                account.AccountName = request.AccountName;
                account.BankName = request.BankName;
                account.DateModified = DateTime.UtcNow;

                await unitOfWork.SaveAsync();
                return Result<string>.Success("Updated", "Successfully");
            }
        }
    }
}
