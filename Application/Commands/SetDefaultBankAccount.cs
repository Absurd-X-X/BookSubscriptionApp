using Application.Common.Dtos;
using Application.Common.Repositories;
using Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace Application.Commands
{
    public class SetDefaultBankAccount
    {
        public record SetDefaultBankAccountCommand(
            Guid BankAccountId,
            Guid UserId) : IRequest<Result<string>>;

        public class SetDefaultBankAccountHandler(
            IBankAccountRepository bankAccountRepository,
            IHttpContextAccessor httpContextAccessor,
            IAuditLogRepository auditLogRepository,
            IUserRepository userRepository,
            IUnitOfWork unitOfWork)
            : IRequestHandler<SetDefaultBankAccountCommand, Result<string>>
        {
            public async Task<Result<string>> Handle(
                SetDefaultBankAccountCommand request,
                CancellationToken cancellationToken)
            {
                var user = await userRepository.GetAsync(request.UserId);
                if (user == null)
                    return Result<string>.Failure("User not found");

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



                string? ipAddress = httpContextAccessor
                .HttpContext?
                .Connection
                .RemoteIpAddress?
                .ToString();


                var audit = new AuditLog
                {
                    ActionType = "Set Default Bank Account",
                    Description = $"Default bank account updated for user {user.UserName}",
                    Icon = "👍",
                    IpAddress = ipAddress!,
                    UserRole = user.Role,
                    UserId = user.Id,
                    ResourceType = ResourceType.System,
                    ResourceId = target.Id,
                };

                await auditLogRepository.AddAsync(audit);

                await unitOfWork.SaveAsync();

                return Result<string>.Success(
                    "Default bank account updated!", "updated");
            }
        }
    }
}