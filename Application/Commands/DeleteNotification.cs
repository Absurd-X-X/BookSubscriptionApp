using Application.Common.Dtos;
using Application.Common.Repositories;
using Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace Application.Commands
{
    public class DeleteNotification
    {
        public record DeleteNotificationCommand(Guid Id) : IRequest<Result<bool>>;

        public class DeleteNotificationHandler(INotificationRepository notificationRepository,
            IAuditLogRepository auditLogRepository,
            IHttpContextAccessor httpContextAccessor
            ) : IRequestHandler<DeleteNotificationCommand, Result<bool>>
        {
            public async Task<Result<bool>> Handle(DeleteNotificationCommand request, CancellationToken cancellationToken)
            {
                await notificationRepository.SoftDeleteAsync(request.Id);

                var notification = await notificationRepository.GetById(request.Id);

                if (notification is null)
                    return Result<bool>.Failure("Notification with this id not found");


                string? ipAddress = httpContextAccessor
                .HttpContext?
                .Connection
                .RemoteIpAddress?
                .ToString();


                var audit = new AuditLog
                {
                    ActionType = "Delete",
                    Description = $"Notification was deleted successfully",
                    Icon = "❌",
                    IpAddress = ipAddress!,
                    UserRole = notification.User.Role,
                    UserId = notification.User.Id,
                    ResourceType = ResourceType.System,
                    ResourceId = notification.Id
                };

                await auditLogRepository.AddAsync(audit);

                await auditLogRepository.AddAsync(audit);
                return Result<bool>.Success(true, "Deleted");
            }
        }
    }
}