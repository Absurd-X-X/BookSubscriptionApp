using Application.Common.Dtos;
using Application.Common.Pagenation;
using Application.Common.Repositories;
using MediatR;

namespace Application.Queries
{
    public class GetReaderActivities
    {
        public record GetReaderActivitiesQuery(
            int Page,
            int PageSize,
            Guid UserId,
            string? Search = null,
            string? ActionType = null,
            DateTime? DateFrom = null,
            DateTime? DateTo = null
        ) : IRequest<Result<PagenatedList<GetReaderActivitiesResponse>>>;

        public class GetReaderActivitiesHandler(IAuditLogRepository auditLog) : IRequestHandler<GetReaderActivitiesQuery, Result<PagenatedList<GetReaderActivitiesResponse>>>
        {
            public async Task<Result<PagenatedList<GetReaderActivitiesResponse>>> Handle(GetReaderActivitiesQuery request, CancellationToken cancellationToken)
            {
                var filter = new AuditLogFilter(request.Search, request.ActionType, request.DateFrom, request.DateTo);

                var auditlogs = await auditLog.GetAllByUserIdAsync(
                    new PageRequest { Page = request.Page, PageSize = request.PageSize },
                    true,
                    filter,
                    request.UserId
                    );

                var audits = auditlogs.Items.Select(v => new GetReaderActivitiesResponse(
                    v.Id,
                    v.Icon,
                    v.UserId,
                    v.UserRole,
                    v.ActionType,
                    v.Description,
                    v.Timestamp,
                    v.User.UserName,
                    v.User.Role,
                    v.User.ImageUrl ?? "none",
                    v.IpAddress
                    )).ToList();

                var auditsPage = new PagenatedList<GetReaderActivitiesResponse>
                {
                    Items = audits,
                    TotalCount = auditlogs.TotalCount,
                    Page = auditlogs.Page,
                    PageSize = auditlogs.PageSize
                };

                return Result<PagenatedList<GetReaderActivitiesResponse>>.Success(auditsPage, "Retrieved");
            }
        }
        public record AuditLogFilter(string? Search, string? ActionType, DateTime? DateFrom, DateTime? DateTo);

        public record GetReaderActivitiesResponse(
                Guid Id,
                string Icon,
                Guid UserId,
                string UserRole,
                string ActionType,
                string Description,
                DateTime Timestamp,
                string UserName,
                string Role,
                string ImageUrl,
                string IpAddress);
    }
}