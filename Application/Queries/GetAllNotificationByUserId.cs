using Application.Common.Dtos;
using Application.Common.Repositories;
using MediatR;

namespace Application.Queries
{
    public class GetAllNotificationByUserId
    {
        public record GetAllNotificationQuery(Guid UserId) : IRequest<Result<IEnumerable<GetAllNotificationResponse>>>;

        public class GetAllNotificationHandler(INotificationRepository notificationRepository) : 
            IRequestHandler<GetAllNotificationQuery, Result<IEnumerable<GetAllNotificationResponse>>>
        {
            public async Task<Result<IEnumerable<GetAllNotificationResponse>>> Handle(GetAllNotificationQuery request, CancellationToken cancellationToken)
            {
                var notifications = await notificationRepository.GetAllNotificationtAsync(request.UserId);

                var notificationData = notifications.Where(x => !x.IsDeleted)
                    .Select(y => new GetAllNotificationResponse(
                        y.Id,
                        y.Title,
                        y.Message,
                        y.Ref,
                        y.IsRead,
                        y.DateCreated
                        )).ToList();

                return Result<IEnumerable<GetAllNotificationResponse>>.Success(notificationData, "Successfully retrieved");
            }
        }

        public record GetAllNotificationResponse(Guid Id, string Title, string Message, string Ref, bool IsRead, DateTime DateCreated);
    }
}
