using Application.Common.Dtos;
using Application.Common.Repositories;
using Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace Application.Command
{
    public class AutoRenewSubscription
    {
        public record AutoRenewCommand(bool AutoRenew, Guid Id) : IRequest<Result<string>>;

        public class AutoRenewHandler(IUnitOfWork unitOfWork,
            IHttpContextAccessor httpContextAccessor,
            IAuditLogRepository auditLogRepository,
            ISubscriptionRepository subscriptionRepository) : IRequestHandler<AutoRenewCommand, Result<string>>
        {
            public async Task<Result<string>> Handle(AutoRenewCommand request, CancellationToken cancellationToken)
            {
                var sub = await subscriptionRepository.GetAsync(request.Id);

                if (sub == null)
                {
                    return Result<string>.Failure("Not found brosky");
                }

                sub.AutoRenewal = true;

                string? ipAddress = httpContextAccessor
               .HttpContext?
               .Connection
               .RemoteIpAddress?
               .ToString();


                var audit = new AuditLog
                {
                    ActionType = "Create",
                    Description = $"{sub.Reader.Name}'s sub was renewed successfully",
                    Icon = "🔔",
                    IpAddress = ipAddress!,
                    UserRole = sub.Reader.User.Role,
                    UserId = sub.Reader.UserId,
                    ResourceType = ResourceType.Reader,
                    ResourceId = sub.Id,
                };

                await auditLogRepository.AddAsync(audit);

                await unitOfWork.SaveAsync();
                return Result<string>.Success("Retrieved", "Brosky");
            }
        }
    }
}
