using Application.Common.Pagenation;
using Application.Common.Repositories;
using Application.Common.Dtos;
using MediatR;

namespace Application.Queries
{
    public class GetCurrentlyReading
    {
        public record GetCurrentlyReadingQuery(
            Guid UserId,
            int Page,
            int PageSize,
            string? Search,
            string? SortBy,
            string? Filter
            ) : IRequest<Result<GetCurrentlyReadingResponse>>;

        public class GetCurrentlyReadingHandler(
            IReadingProgressRepository readingProgressRepository,
            INotificationRepository notificationRepository
            ) : IRequestHandler<GetCurrentlyReadingQuery, Result<GetCurrentlyReadingResponse>>
        {
            async Task<Result<GetCurrentlyReadingResponse>> IRequestHandler<GetCurrentlyReadingQuery, Result<GetCurrentlyReadingResponse>>.
                Handle(GetCurrentlyReadingQuery request, CancellationToken cancellationToken)
            {
                var userId = request.UserId;

                var pageRequest = new PageRequest
                {
                    Page = request.Page,
                    PageSize = request.PageSize
                };

                var paged = await readingProgressRepository.GetCurrentlyReadingPagedAsync(
                    userId,
                    pageRequest,
                    usePaging: true,
                    request.Search,
                    request.SortBy,
                    request.Filter);

                var activeCount = await readingProgressRepository.GetCurrentlyReadingCountAsync(userId);
                var totalMinutes = await readingProgressRepository.GetTotalReadingMinutesAsync(userId);
                var maxStreak = await readingProgressRepository.GetMaxCurrentStreakAsync(userId);

                var notifications = await notificationRepository.GetAllNotificationtAsync(userId);
                var unreadCount = await notificationRepository.GetUnreadCountAsync(userId);

                var items = paged.Items.Select(x =>
                {
                    int? minutesLeft = null;

                    if (x.Book.Pages > 0 && x.TotalPagesRead > 0)
                    {
                        var remainingPages = x.Book.Pages - x.CurrentPage;
                        if (remainingPages > 0)
                        {
                            var paceMinutesPerPage = (double)x.TotalMinutesRead / x.TotalPagesRead;
                            minutesLeft = (int)Math.Round(remainingPages * paceMinutesPerPage);
                        }
                        else
                        {
                            minutesLeft = 0;
                        }
                    }

                    return new CurrentlyReadingItemResponse(
                        x.Id,
                        x.BookId,
                        x.Book.Title,
                        x.Book.Author,
                        x.Book.BookCoverUrl,
                        x.ProgressPercentage,
                        x.LastReadDate,
                        x.DateCreated,
                        x.TotalMinutesRead,
                        minutesLeft
                    );
                }).ToList();

                var response = new GetCurrentlyReadingResponse(
                    items,
                    paged.Page,
                    paged.PageSize,
                    paged.TotalCount,
                    activeCount,
                    totalMinutes,
                    maxStreak,
                    notifications
                        .OrderByDescending(x => x.DateModified)
                        .Take(5)
                        .Select(x => new NotificationItemResponse(
                            x.Id,
                            x.Title,
                            x.Message,
                            x.Type.ToString(),
                            x.IsRead,
                            x.DateModified
                        )).ToList(),
                    unreadCount
                );

                return Result<GetCurrentlyReadingResponse>.Success(response, "Retrieved");
            }
        }

        public record GetCurrentlyReadingResponse(
            List<CurrentlyReadingItemResponse> Items,
            int Page,
            int PageSize,
            long TotalCount,
            int ActiveBooksCount,
            int TotalReadingMinutes,
            int ReadingStreakDays,
            List<NotificationItemResponse> Notifications,
            int UnreadNotificationCount
            );

        public record CurrentlyReadingItemResponse(
            Guid ReadingProgressId,
            Guid BookId,
            string Title,
            string Author,
            string BookCoverUrl,
            double ProgressPercentage,
            DateTime? LastReadDate,
            DateTime StartedOn,
            int TotalMinutesRead,
            int? EstimatedMinutesLeft
            );

        public record NotificationItemResponse(
            Guid Id,
            string Title,
            string Message,
            string Type,
            bool IsRead,
            DateTime DateModified
            );
    }
}