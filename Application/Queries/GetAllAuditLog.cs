using Application.Common.Dtos;
using Application.Common.Pagenation;
using Application.Common.Repositories;
using MediatR;

namespace Application.Queries
{
    public class GetAllAuditLog
    {
        public record GetAllAuditLogQuery(int Page, int PageSize) : IRequest<Result<PagenatedList<GetAllAuditLogResponse>>>;

        public class GetAuditLogHandler(IAuditLogRepository auditLog) : IRequestHandler<GetAllAuditLogQuery, Result<PagenatedList<GetAllAuditLogResponse>>>
        {
            public async Task<Result<PagenatedList<GetAllAuditLogResponse>>> Handle(GetAllAuditLogQuery request, CancellationToken cancellationToken)
            {
                var auditlogs = await auditLog.GetAllAsync(new PageRequest { Page = request.Page, PageSize = request.PageSize }, true);

                if (auditlogs.TotalCount < 1)
                {
                    return Result<PagenatedList<GetAllAuditLogResponse>>.Failure("No log found!");
                }

                var audits = auditlogs.Items.Select(v => new GetAllAuditLogResponse(
                    v.Id,
                    v.Icon,
                    v.UserRole,
                    v.ActionType,
                    v.Description,
                    v.Timestamp,
                    v.User.UserName,
                    v.User.Role,
                    v.User.ImageUrl ?? "none",
                    v.IpAddress
                    )).ToList();

                var auditsPage = new PagenatedList<GetAllAuditLogResponse>
                {
                    Items = audits,
                    TotalCount = auditlogs.TotalCount,
                    Page = auditlogs.Page,
                    PageSize = auditlogs.PageSize
                };

                return Result<PagenatedList<GetAllAuditLogResponse>>.Success(auditsPage, "Retrieved");
            }
        }

        public record GetAllAuditLogResponse(
            Guid Id,
            string Icon,
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
