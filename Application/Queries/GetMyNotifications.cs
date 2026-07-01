using Application.Common.Dtos;
using Application.Common.Repositories;
using MediatR;

namespace Application.Queries
{
    public class GetMyNotifications
    {
        public record GetMyNotificationsQuery(
            Guid UserId)
            : IRequest<Result<List<GetMyNotificationsResponse>>>;

        public class GetMyNotificationsHandler(
            INotificationRepository notificationRepository)
            : IRequestHandler<GetMyNotificationsQuery,
                Result<List<GetMyNotificationsResponse>>>
        {
            public async Task<Result<List<GetMyNotificationsResponse>>> Handle(
                GetMyNotificationsQuery request,
                CancellationToken cancellationToken)
            {
                var notifications = await notificationRepository
                    .GetAllNotificationtAsync(request.UserId);

                var response = notifications.Select(n =>
                    new GetMyNotificationsResponse(
                        n.Id, n.Title, n.Message,
                        n.Type.ToString(), n.IsRead,
                        n.Ref, n.DateCreated))
                    .ToList();

                return Result<List<GetMyNotificationsResponse>>
                    .Success(response, "Success");
            }
        }

        public record GetMyNotificationsResponse(
            Guid Id, string Title, string Message,
            string Type, bool IsRead,
            string Ref, DateTime DateCreated);
    }
}