using Application.Common.Dtos;
using Application.Common.Repositories;
using Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace Application.Commands
{
    public class MarkNotificationRead
    {
        public record MarkNotificationReadCommand(Guid Id, Guid UserId, bool Read) : IRequest<Result<bool>>;

        public class MarkNotificationReadHandler(INotificationRepository notificationRepository,
            IUserRepository userRepository,
            IHttpContextAccessor httpContextAccessor,
            IAuditLogRepository auditLogRepository)
            : IRequestHandler<MarkNotificationReadCommand, Result<bool>>
        {
            public async Task<Result<bool>> Handle(MarkNotificationReadCommand request, CancellationToken cancellationToken)
            {
                if (request.Read)
                    await notificationRepository.MarkAsReadAsync(request.Id);
                else
                    await notificationRepository.MarkAsUnreadAsync(request.Id);

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
                    Description = $"Notification was marked as { (request.Read ? "read" : "unread") } successfully",
                    Icon = "✔️",
                    IpAddress = ipAddress!,
                    UserRole = user.Role,
                    UserId = user.Id,
                    ResourceType = ResourceType.System,
                    ResourceId = request.Id,
                };

                await auditLogRepository.AddAsync(audit);
                return Result<bool>.Success(true, "Updated");
            }
        }
    }
}