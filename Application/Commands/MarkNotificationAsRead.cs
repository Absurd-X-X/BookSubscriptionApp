using Application.Common.Dtos;
using Application.Common.Repositories;
using MediatR;

namespace Application.Command
{
    public class MarkNotificationAsRead
    {
        public record MarkNotificationAsReadCommand(
            Guid NotificationId,
            bool MarkAll
            ) : IRequest<Result<string>>;

        public class MarkNotificationAsReadHAndler(
            INotificationRepository notificationRepository,
            ICurrentUser currentUser,
            IUnitOfWork unitOfWork,
            IUserRepository userRepository
            ) : IRequestHandler<MarkNotificationAsReadCommand, Result<string>>
        {
            public async Task<Result<string>> Handle(MarkNotificationAsReadCommand request, CancellationToken cancellationToken)
            {
                var userId = currentUser.GetCurrentUser();
                var user = await userRepository.GetAsync( userId );

                if (user is null)
                    return Result<string>.Failure("User not found");

                var notifications = await notificationRepository.GetAllNotificationtAsync(user.Id);

                if (request.MarkAll)
                {
                    var notification = await notificationRepository.GetById(request.NotificationId);

                    if (notification is null)
                        return Result<string>.Failure("Notification not found");

                    var read = notifications.Where(x => !x.IsRead).ToList();

                    foreach(var item in read)
                    {
                        item.IsRead = true;
                        item.DateModified = DateTime.UtcNow;
                    }
                }

                else
                {
                    var notification = await notificationRepository.GetById(request.NotificationId);

                    if (notification is null)
                        return Result<string>.Failure("Not found");

                    if (notification.UserId == user.Id)
                        return Result<string>.Failure("Unknown personnel");

                    notification.IsRead = true;
                    notification.DateModified = DateTime.UtcNow;
                }
                await unitOfWork.SaveAsync();
                return Result<string>.Success("All", "Marked");
            }
        }
    }
}
