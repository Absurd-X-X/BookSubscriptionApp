using Application.Common.Dtos;
using Application.Common.Repositories;
using Domain.Entities;
using Domain.Enums;
using MediatR;

namespace Application.Queries
{
    public class GetTransactionById
    {
        public record GetTransactionByIdQuery(Guid Id) : IRequest<Result<GetTransactionByIdResponse>>;

        public class GetTransactionByIdHandler(
            IWalletTransactionRepository transactionRepository) : IRequestHandler<GetTransactionByIdQuery, Result<GetTransactionByIdResponse>>
        {
            public async Task<Result<GetTransactionByIdResponse>> Handle(GetTransactionByIdQuery request, CancellationToken cancellationToken)
            {
                var transaction = await transactionRepository.GetByIdAsync(request.Id);
                if (transaction == null)
                    return Result<GetTransactionByIdResponse>.Failure("Transaction not found.");
                var response = new GetTransactionByIdResponse(
                    transaction.Id,
                    transaction.WalletId,
                    transaction.Balance,
                    transaction.Type,
                    transaction.Status,
                    transaction.PaystackReference,
                    transaction.Description,
                    transaction.BalanceBefore,
                    transaction.BalanceAfter,
                    transaction.CreatedBy,
                    transaction.DateCreated,
                    transaction.Wallet
                    );
                return Result<GetTransactionByIdResponse>.Success(response, "Retrieved");
            }
        }

        public record GetTransactionByIdResponse(
            Guid Id, 
            Guid WalletId, 
            decimal Balance, 
            TransactionType Type,
            WalletTransactionStatus Status, 
            string? PaystackReference,
            string Description, 
            decimal BalanceBefore, 
            decimal BalanceAfter, 
            string CreatedBy, 
            DateTime DateCreated,
            Wallet Wallet
            );

    }
}
