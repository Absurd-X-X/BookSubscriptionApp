using Application.Common.Dtos;
using Application.Common.Pagenation;
using Application.Common.Repositories;
using MediatR;

namespace Application.Queries
{
    public class GetAuditLogByRole
    {
        public record GetAuditLogByRoleQuery(string Role, int Page, int PageSize) : IRequest<Result<IEnumerable<GetAuditLogByRoleResponse>>>;

        public class GetAuditLogHandler(IAuditLogRepository auditLog) : IRequestHandler<GetAuditLogByRoleQuery, Result<IEnumerable<GetAuditLogByRoleResponse>>>
        {
            public async Task<Result<IEnumerable<GetAuditLogByRoleResponse>>> Handle(GetAuditLogByRoleQuery request, CancellationToken cancellationToken)
            {
                var auditlogs = await auditLog.GetByUserRoleAsync(request.Role, new PageRequest { Page = request.Page, PageSize = request.PageSize }, true);

                if (auditlogs.TotalCount < 1)
                {
                    return Result<IEnumerable<GetAuditLogByRoleResponse>>.Failure("No log found!");
                }

                var audits = auditlogs.Items.Select(v => new GetAuditLogByRoleResponse(
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

                return Result<IEnumerable<GetAuditLogByRoleResponse>>.Success(audits, "Retrieved");
            }
        }

        public record GetAuditLogByRoleResponse(
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
