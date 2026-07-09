using Application.Common.Dtos;
using Application.Common.Pagenation;
using Application.Common.Repositories;
using Domain.Entities;
using MediatR;

namespace Application.Queries
{
    public class GetLibraryAuditLogs
    {
        public record GetLibraryAuditLogsQuery(int Page, int PageSize) : IRequest<Result<GetLibraryAuditLogsResponse>>;

        public class GetLibraryAuditLogsHandler(
            IAuditLogRepository auditLogRepository,
            IBookRepository bookRepository
            ) : IRequestHandler<GetLibraryAuditLogsQuery, Result<GetLibraryAuditLogsResponse>>
        {
            public async Task<Result<GetLibraryAuditLogsResponse>> Handle(GetLibraryAuditLogsQuery request, CancellationToken cancellationToken)
            {
                var paged = await auditLogRepository.GetAllAsync(new PageRequest
                {
                    Page = request.Page,
                    PageSize = request.PageSize
                }, true);

                var allLogsResult = await auditLogRepository.GetAllAsync(new PageRequest
                {
                    Page = 1,
                    PageSize = int.MaxValue
                }, false);

                var allLogs = allLogsResult.Items;

                var rows = new List<AuditLogRow>();
                foreach (var log in paged.Items)
                {
                    string? resourceTitle = null;
                    string? resourceAuthor = null;

                    if (log.ResourceType == ResourceType.Book && log.ResourceId.HasValue)
                    {
                        var book = await bookRepository.GetByIdAsync(log.ResourceId.Value);
                        resourceTitle = book?.Title;
                        resourceAuthor = book?.Author;
                    }

                    rows.Add(new AuditLogRow(
                        log.Id,
                        log.Icon,
                        log.UserRole,
                        log.ActionType,
                        log.Description,
                        log.Timestamp,
                        log.User.UserName,
                        log.User.Role,
                        log.User.ImageUrl ?? "none",
                        log.IpAddress,
                        log.ResourceType.ToString(),
                        resourceTitle,
                        resourceAuthor
                    ));
                }

                var now = DateTime.UtcNow;
                var thisMonthStart = new DateTime(now.Year, now.Month, 1);
                var lastMonthStart = thisMonthStart.AddMonths(-1);
                var lastMonthEnd = thisMonthStart;

                var thisMonthLogs = allLogs.Where(x => x.Timestamp >= thisMonthStart).ToList();
                var lastMonthLogs = allLogs.Where(x => x.Timestamp >= lastMonthStart && x.Timestamp < lastMonthEnd).ToList();

                double PercentChange(int current, int previous)
                    => previous == 0 ? (current > 0 ? 100 : 0) : Math.Round((current - previous) * 100.0 / previous, 1);

                int totalActivities = allLogs.Count();
                int bookActivities = allLogs.Count(x => x.ResourceType == ResourceType.Book);
                int readerActivities = allLogs.Count(x => x.ResourceType == ResourceType.Reader);
                int reviewActivities = allLogs.Count(x => x.ResourceType == ResourceType.Review);
                int systemActivities = allLogs.Count(x => x.ResourceType == ResourceType.System);

                int totalThisMonth = thisMonthLogs.Count;
                int totalLastMonth = lastMonthLogs.Count;
                int bookThisMonth = thisMonthLogs.Count(x => x.ResourceType == ResourceType.Book);
                int bookLastMonth = lastMonthLogs.Count(x => x.ResourceType == ResourceType.Book);
                int readerThisMonth = thisMonthLogs.Count(x => x.ResourceType == ResourceType.Reader);
                int readerLastMonth = lastMonthLogs.Count(x => x.ResourceType == ResourceType.Reader);
                int reviewThisMonth = thisMonthLogs.Count(x => x.ResourceType == ResourceType.Review);
                int reviewLastMonth = lastMonthLogs.Count(x => x.ResourceType == ResourceType.Review);
                int systemThisMonth = thisMonthLogs.Count(x => x.ResourceType == ResourceType.System);
                int systemLastMonth = lastMonthLogs.Count(x => x.ResourceType == ResourceType.System);

                var topUsers = allLogs
                    .GroupBy(x => new { x.UserId, x.User.UserName })
                    .Select(g => new { g.Key.UserName, Count = g.Count() })
                    .OrderByDescending(x => x.Count)
                    .Take(6)
                    .ToList();

                int maxUserCount = topUsers.Any() ? topUsers.Max(x => x.Count) : 1;

                var topUserItems = topUsers
                    .Select(x => new TopUserItem(
                        x.UserName,
                        x.Count,
                        maxUserCount == 0 ? 0 : Math.Round(x.Count * 100.0 / maxUserCount, 1)
                    ))
                    .ToList();

                var response = new GetLibraryAuditLogsResponse(
                    Items: rows,
                    Page: paged.Page,
                    PageSize: paged.PageSize,
                    TotalCount: paged.TotalCount,
                    TotalActivities: totalActivities,
                    BookActivities: bookActivities,
                    ReaderActivities: readerActivities,
                    ReviewActivities: reviewActivities,
                    SystemActivities: systemActivities,
                    TotalActivitiesChangePercent: PercentChange(totalThisMonth, totalLastMonth),
                    BookActivitiesChangePercent: PercentChange(bookThisMonth, bookLastMonth),
                    ReaderActivitiesChangePercent: PercentChange(readerThisMonth, readerLastMonth),
                    ReviewActivitiesChangePercent: PercentChange(reviewThisMonth, reviewLastMonth),
                    SystemActivitiesChangePercent: PercentChange(systemThisMonth, systemLastMonth),
                    TopUsers: topUserItems
                );

                return Result<GetLibraryAuditLogsResponse>.Success(response, "Retrieved");
            }
        }

        public record GetLibraryAuditLogsResponse(
            List<AuditLogRow> Items,
            int Page,
            int PageSize,
            long TotalCount,
            int TotalActivities,
            int BookActivities,
            int ReaderActivities,
            int ReviewActivities,
            int SystemActivities,
            double TotalActivitiesChangePercent,
            double BookActivitiesChangePercent,
            double ReaderActivitiesChangePercent,
            double ReviewActivitiesChangePercent,
            double SystemActivitiesChangePercent,
            List<TopUserItem> TopUsers
        );

        public record AuditLogRow(
            Guid Id,
            string Icon,
            string UserRole,
            string ActionType,
            string Description,
            DateTime Timestamp,
            string UserName,
            string Role,
            string ImageUrl,
            string IpAddress,
            string ResourceType,
            string? ResourceTitle,
            string? ResourceAuthor
        );

        public record TopUserItem(string UserName, int Count, double PercentOfMax);
    }
}