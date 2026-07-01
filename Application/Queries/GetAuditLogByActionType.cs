using Application.Common.Dtos;
using Application.Common.Pagenation;
using Application.Common.Repositories;
using MediatR;

namespace Application.Queries
{
    public class GetAuditLogByActionType
    {
        public record GetAuditLogByActionTypeQuery(string ActionType, int Page, int PageSize) : IRequest<Result<IEnumerable<GetAuditLogByActionTypeResponse>>>;

        public class GetAuditLogHandler(IAuditLogRepository auditLog) : IRequestHandler<GetAuditLogByActionTypeQuery, Result<IEnumerable<GetAuditLogByActionTypeResponse>>>
        {
            public async Task<Result<IEnumerable<GetAuditLogByActionTypeResponse>>> Handle(GetAuditLogByActionTypeQuery request, CancellationToken cancellationToken)
            {
                var auditlogs = await auditLog.GetByActionTypeAsync(request.ActionType, new PageRequest { Page = request.Page, PageSize = request.PageSize }, true);

                if (auditlogs.TotalCount < 1)
                {
                    return Result<IEnumerable<GetAuditLogByActionTypeResponse>>.Failure("No log found!");
                }

                var audits = auditlogs.Items.Select(v => new GetAuditLogByActionTypeResponse(
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

                return Result<IEnumerable<GetAuditLogByActionTypeResponse>>.Success(audits, "Retrieved");
            }
        }

        public record GetAuditLogByActionTypeResponse(
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
