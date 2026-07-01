using Application.Queries;

namespace Application.ViewModels
{
    public class UserDetailsAuditLog
    {
        public GetUserDetails.GetUserDetailsResponse UserDetails { get; set; } = null!;
        public IEnumerable<GetAuditLog.GetAuditLogResponse> AuditLogs { get; set; } = null!;
    }
}
