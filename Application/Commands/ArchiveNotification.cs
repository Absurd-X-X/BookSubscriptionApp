using Application.Common.Dtos;
using Application.Common.Repositories;
using Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace Application.Commands
{
    public class ArchiveNotification
    {
        public record ArchiveNotificationCommand(Guid Id) : IRequest<Result<bool>>;

        public class ArchiveNotificationHandler(INotificationRepository notificationRepository,
            IHttpContextAccessor httpContextAccessor,
            IAuditLogRepository auditLogRepository,
            IUnitOfWork unitOfWork)
            : IRequestHandler<ArchiveNotificationCommand, Result<bool>>
        {
            public async Task<Result<bool>> Handle(ArchiveNotificationCommand request, CancellationToken cancellationToken)
            {
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
                    ActionType = "Archive",
                    Description = $"Notification was archived successfully",
                    Icon = "🔒",
                    IpAddress = ipAddress!,
                    UserRole = notification.User.Role,
                    UserId = notification.User.Id,
                    ResourceType = ResourceType.System,
                    ResourceId = notification.Id
                };

                await auditLogRepository.AddAsync(audit);

                await notificationRepository.ArchiveAsync(request.Id);

                await unitOfWork.SaveAsync();
                return Result<bool>.Success(true, "Archived");
            }
        }
    }
}