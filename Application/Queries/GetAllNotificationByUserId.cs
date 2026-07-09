using Application.Common.Dtos;
using Application.Common.Repositories;
using MediatR;

namespace Application.Queries
{
    public class GetAllNotificationByUserId
    {
        public record GetAllNotificationQuery(Guid UserId) : IRequest<Result<NotificationPageResponse>>;

        public class GetAllNotificationHandler(INotificationRepository notificationRepository) :
            IRequestHandler<GetAllNotificationQuery, Result<NotificationPageResponse>>
        {
            public async Task<Result<NotificationPageResponse>> Handle(GetAllNotificationQuery request, CancellationToken cancellationToken)
            {
                var notifications = await notificationRepository.GetAllNotificationtAsync(request.UserId);
                var active = notifications
                    .Where(x => !x.IsDeleted && !x.IsArchived)
                    .OrderByDescending(x => x.DateCreated)
                    .ToList();

                var totalNotifications = active.Count;
                var unreadNotifications = active.Count(x => !x.IsRead);
                var readNotifications = active.Count(x => x.IsRead);

                var actionRequired = active.Count(x => !x.IsRead && x.Type == Domain.Enums.NotificationType.Reminder);

                var today = DateTime.UtcNow.Date;
                var resolvedToday = active.Count(x => x.IsRead && x.DateModified.Date == today);

                var items = active.Select(y => new GetAllNotificationResponse(
                    y.Id, y.Title, y.Message, y.Ref, y.IsRead, y.Type.ToString(), y.DateCreated
                    )).ToList();

                var categoryBreakdown = active
                    .GroupBy(x => x.Type)
                    .Select(g => new CategoryCountDto(
                        g.Key.ToString(),
                        g.Count(),
                        totalNotifications > 0 ? (int)Math.Round((double)g.Count() / totalNotifications * 100) : 0))
                    .ToList();

                var response = new NotificationPageResponse(
                    TotalNotifications: totalNotifications,
                    UnreadNotifications: unreadNotifications,
                    ReadNotifications: readNotifications,
                    ActionRequired: actionRequired,
                    ResolvedToday: resolvedToday,
                    Items: items,
                    CategoryBreakdown: categoryBreakdown
                );

                return Result<NotificationPageResponse>.Success(response, "Successfully retrieved");
            }
        }

        // ⬇ REPLACE the three old records (GetAllNotificationResponse, and add the two new ones) with these
        public record GetAllNotificationResponse(Guid Id, string Title, string Message, string Ref, bool IsRead, string Category, DateTime DateCreated);
        public record CategoryCountDto(string Category, int Count, int Percentage);
        public record NotificationPageResponse(
            int TotalNotifications,
            int UnreadNotifications,
            int ReadNotifications,
            int ActionRequired,
            int ResolvedToday,
            List<GetAllNotificationResponse> Items,
            List<CategoryCountDto> CategoryBreakdown
        );
    }
}