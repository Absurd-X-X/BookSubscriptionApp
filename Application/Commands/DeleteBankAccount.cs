using Application.Common.Dtos;
using Application.Common.Repositories;
using Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.SignalR;

namespace Application.Command
{
    public class DeleteBankAccount
    {
        public record DeleteBankAccountCommand(
            Guid BankAccountId) : IRequest<Result<string>>;

        public class DeleteBankAccountHandler(
            IBankAccountRepository bankAccountRepository,
            IUserRepository userRepository,
            IAuditLogRepository auditLogRepository,
            IHttpContextAccessor httpContextAccessor,
            ICurrentUser currentUser,
            IUnitOfWork unitOfWork) : IRequestHandler<DeleteBankAccountCommand, Result<string>>
        {
            public async Task<Result<string>> Handle(DeleteBankAccountCommand request, CancellationToken cancellationToken)
            {
                var account = await bankAccountRepository.GetByIdAsync(request.BankAccountId);

                if (account is null)
                    return Result<string>.Failure("Account not found");
                var userId = currentUser.GetCurrentUser();

                var user = await userRepository.GetAsync(userId);
                if (user is null)
                    return Result<string>.Failure("Unauthorized");


                if ( account.UserId != userId)
                    return Result<string>.Failure("Unauthorized");

                account.IsDeleted = true;
                account.DateModified = DateTime.UtcNow;

                string? ipAddress = httpContextAccessor
              .HttpContext?
              .Connection
              .RemoteIpAddress?
              .ToString();


                var audit = new AuditLog
                {
                    ActionType = "Delete",
                    Description = $"Bank Account sub deleted successfully",
                    Icon = "❌",
                    IpAddress = ipAddress!,
                    UserRole = user.Role,
                    UserId = user.Id,
                    ResourceType = ResourceType.System,
                    ResourceId = account.Id,
                };

                await auditLogRepository.AddAsync(audit);

                await unitOfWork.SaveAsync();

                return Result<string>.Success("Deleted", "Successfullly");
            }
        }
    }
}
