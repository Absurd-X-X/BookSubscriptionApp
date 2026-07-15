using Application.Common.Dtos;
using Application.Common.Repositories;
using Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace Application.Command
{
    public class Subscribe
    {
        public record SubscribeCommand(
            Guid UserId,
            Guid SubscriptionTypeId
            ) : IRequest<Result<string>>;

        public class SubscribeHandler(
            ISubscriptionRepository subscriptionRepository,
            IUserRepository userRepository,
            IReaderRepository readerRepository,
            ISubscriptionTypeRepository subscriptionTypeRepository,
            IAuditLogRepository auditLogRepository,
            IHttpContextAccessor httpContextAccessor,
            IUnitOfWork unitOfWork
            ) : IRequestHandler<SubscribeCommand, Result<string>>
        {
            public async Task<Result<string>> Handle(SubscribeCommand request, CancellationToken cancellationToken)
            {
                var user = await userRepository.GetAsync(request.UserId);
                if (user is null)
                    return Result<string>.Failure("User not found");

                var reader = await readerRepository.GetByEmailAsync(user.Email);

                if (reader is null)
                    return Result<string>.Failure("Reader not found");

                var subscriptionType = await subscriptionTypeRepository.GetByIdAsync(request.SubscriptionTypeId);

                if (subscriptionType is null)
                    return Result<string>.Failure("Subscription type not found");

                if (user.Wallet!.Balance < subscriptionType.Cost)
                    return Result<string>.Failure("Insufficient balance");

                var getAdmin = await userRepository.GetAsync("admin@gmail.com");

                if (getAdmin is null)
                {
                    return Result<string>.Failure("Admin not found");
                }

                var sub = await subscriptionRepository.GetByReaderIdAsync(reader.Id, true);

                if (sub is not null)
                {
                    if (sub.SubscriptionTypeId == request.SubscriptionTypeId)
                    {
                        return Result<string>.Failure("You are already subscribed to this plan");
                    }

                    sub.IsActive = false;
                }

                user.Wallet.Balance -= subscriptionType.Cost;
                getAdmin.Wallet!.Balance += subscriptionType.Cost;
                subscriptionType.SubscriptionDate = DateTime.UtcNow;

                var subscription = subscriptionRepository.AddAsync(new Subscription
                {
                    AutoRenewal = false,
                    CreatedBy = reader.Email,
                    IsActive = true,
                    ReaderId = reader.Id,
                    SubscriptionTypeId = request.SubscriptionTypeId
                });

                string? ipAddress = httpContextAccessor
                    .HttpContext?
                    .Connection
                    .RemoteIpAddress?
                    .ToString();

                var audit = new AuditLog
                {
                    ActionType = sub is not null ? "SwitchSubscription" : "Subscribe",
                    Description = sub is not null
                        ? $"Switched subscription plan"
                        : $"Subscription created successfully",
                    Icon = "👆",
                    IpAddress = ipAddress!,
                    UserRole = user.Role,
                    UserId = user.Id,
                    ResourceType = ResourceType.Reader,
                    ResourceId = reader.Id,
                };

                await auditLogRepository.AddAsync(audit);

                await unitOfWork.SaveAsync();
                return Result<string>.Success("Subscribe", "Successfully");
            }
        }
    }
}