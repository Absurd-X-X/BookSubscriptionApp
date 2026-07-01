using Application.Common.Dtos;
using Application.Common.Repositories;
using Mapster;
using MediatR;

namespace Application.Queries
{
    public class GetBankAccountById
    {
        public record GetBankAccountByIdQuery(Guid Id) : IRequest<Result<GetBankAccountByIdResponse>>;

        public class GetBankAccountByIdHandler(
            IBankAccountRepository bankAccountRepository
            ) : IRequestHandler<GetBankAccountByIdQuery, Result<GetBankAccountByIdResponse>>
        {
            public async Task<Result<GetBankAccountByIdResponse>> Handle(GetBankAccountByIdQuery request, CancellationToken cancellationToken)
            {
                var account = await bankAccountRepository.GetByIdAsync(request.Id);

                if (account is null)
                    return Result<GetBankAccountByIdResponse>.Failure("Not found");

                return Result<GetBankAccountByIdResponse>.
                    Success(account.Adapt<GetBankAccountByIdResponse>(), "Retieved Successfully");
            }
        }

        public record GetBankAccountByIdResponse(
            Guid Id, 
            string AccountName, 
            string AccountNumber, 
            string BankName, 
            bool IsDefault, 
            DateTime DateCreated);
    }
}
