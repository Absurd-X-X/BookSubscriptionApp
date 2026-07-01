using Application.Common.Dtos;
using Application.Common.Repositories;
using Domain.Entities;
using Mapster;
using MediatR;

namespace Application.Queries
{
    public class GetBankAccountByUserId
    {
        public record GetBankAccountByUserIdQuery(Guid UserId) : IRequest<Result<GetBankAccountByUserIdResponse>>;

        public class GetBankAccountByUserIdHandler(
            IBankAccountRepository bankAccountRepository
            ) : IRequestHandler<GetBankAccountByUserIdQuery, Result<GetBankAccountByUserIdResponse>>
        {
            public async Task<Result<GetBankAccountByUserIdResponse>> Handle(GetBankAccountByUserIdQuery request, CancellationToken cancellationToken)
            {
                var account = await bankAccountRepository.GetByUserIdAsync(request.UserId);

                if (account is null)
                    return Result<GetBankAccountByUserIdResponse>.Failure("Not found");

                return Result<GetBankAccountByUserIdResponse>.Success(account.Adapt<GetBankAccountByUserIdResponse>(), "Successful");
            }
        }

        public record GetBankAccountByUserIdResponse(
            Guid Id,
            string AccountName,
            string AccountNumber,
            string BankName,
            bool IsDefault,
            DateTime DateCreated);
    }
}
