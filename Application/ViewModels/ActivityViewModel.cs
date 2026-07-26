using Application.Common.Pagenation;
using Application.Queries;
using static Application.Queries.GetReaderActivities;

namespace Application.ViewModels
{
    public class ActivityViewModel
    {
        public PagenatedList<GetReaderActivitiesResponse> AuditLogs { get; set; } = new PagenatedList<GetReaderActivitiesResponse>();

        public GetAllNotificationByUserId.NotificationPageResponse Notifications { get; set; } = default!;

        public string? Search { get; set; }
        public string? ActionType { get; set; }
        public DateTime? DateFrom { get; set; }
        public DateTime? DateTo { get; set; }
    }
}