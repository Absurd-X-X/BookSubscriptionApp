using Application.Common.Dtos;
using Application.Common.Repositories;
using Mapster;
using MediatR;
using MySqlX.XDevAPI.Common;

namespace Application.Queries
{
    public class GetAllBankAccountOwnedByUser
    {
        public record GetAllBankAccountOwnedByUserQuery(Guid UserId) : 
            IRequest<Result<GetAllBankAccountOwnedByUserResponse>>;

        public class GetAllBankAccountOwnedByUserHandler(
            IBankAccountRepository bankAccountRepository
            ) : IRequestHandler<GetAllBankAccountOwnedByUserQuery, Result<GetAllBankAccountOwnedByUserResponse>>
        {
            public async Task<Result<GetAllBankAccountOwnedByUserResponse>> Handle(GetAllBankAccountOwnedByUserQuery request, 
                CancellationToken cancellationToken)
            {
                var account = await bankAccountRepository.GetAllAccountByUserAsync(request.UserId);

                if (account is null)
                    return Result<GetAllBankAccountOwnedByUserResponse>.Failure("User or bank account not found");

                return Result<GetAllBankAccountOwnedByUserResponse>.Success(account.Adapt<GetAllBankAccountOwnedByUserResponse>(), "Retrieved");
            }
        }

        public record GetAllBankAccountOwnedByUserResponse(Guid UserId, string AccountName, string AccountNumber, string BankName);
    }
}
