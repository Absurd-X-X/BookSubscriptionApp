using Application.Common.Dtos;
using Application.Common.Pagenation;
using Application.Common.Repositories;
using MediatR;

namespace Application.Queries
{
    public class GetAuditLog
    {
        public record GetAuditLogQuery(Guid UserId, int Page, int PageSize) : IRequest<Result<IEnumerable<GetAuditLogResponse>>>;

        public class GetAuditLogHandler(IAuditLogRepository auditLog) : IRequestHandler<GetAuditLogQuery, Result<IEnumerable<GetAuditLogResponse>>>
{
            public async Task<Result<IEnumerable<GetAuditLogResponse>>> Handle(GetAuditLogQuery request, CancellationToken cancellationToken)
            {
                var auditlogs = await auditLog.GetAsync(request.UserId,new PageRequest { Page = request.Page, PageSize = request.PageSize }, true);

                if (auditlogs.TotalCount < 1)
                {
                    return Result<IEnumerable<GetAuditLogResponse>>.Failure("No log found!");
                }

                var audits = auditlogs.Items.Select(v => new GetAuditLogResponse(
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

                return Result<IEnumerable<GetAuditLogResponse>>.Success(audits, "Retrieved");
            }
        }

        public record GetAuditLogResponse(
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
