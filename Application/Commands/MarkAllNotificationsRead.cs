// Application.Commands/MarkAllNotificationsRead.cs
using Application.Common.Dtos;
using Application.Common.Repositories;
using Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace Application.Commands
{
    public class MarkAllNotificationsRead
    {
        public record MarkAllNotificationsReadCommand(Guid UserId) : IRequest<Result<bool>>;

        public class MarkAllNotificationsReadHandler(INotificationRepository notificationRepository,
            IUserRepository userRepository,
            IAuditLogRepository auditLogRepository,
            IHttpContextAccessor httpContextAccessor)
            : IRequestHandler<MarkAllNotificationsReadCommand, Result<bool>>
        {
            public async Task<Result<bool>> Handle(MarkAllNotificationsReadCommand request, CancellationToken cancellationToken)
            {
                await notificationRepository.MarkAllAsReadAsync(request.UserId);

                var user = await userRepository.GetAsync(request.UserId);

                if (user is null)
                    return Result<bool>.Failure("User not found");


                string? ipAddress = httpContextAccessor
                .HttpContext?
                .Connection
                .RemoteIpAddress?
                .ToString();


                var audit = new AuditLog
                {
                    ActionType = "Marked",
                    Description = $"All Notifications were marked as read successfully",
                    Icon = "✔️",
                    IpAddress = ipAddress!,
                    UserRole = user.Role,
                    UserId = user.Id,
                    ResourceType = ResourceType.System,
                    ResourceId = null,
                };

                await auditLogRepository.AddAsync(audit);
                return Result<bool>.Success(true, "All marked as read");
            }
        }
    }
}