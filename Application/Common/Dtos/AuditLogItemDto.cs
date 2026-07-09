using Application.Common.Pagenation;

namespace Application.Common.Dtos
{
    public record AuditLogItemDto(
        Guid Id,
        string Icon,
        string UserName,
        string UserRole,
        string ActionType,
        string Description,
        string Category,
        string? ResourceTitle,
        string? ResourceAuthor,
        string? ResourceImageUrl,
        DateTime Timestamp,
        string IpAddress
    );

    public record ActivitySummaryDto(string Category, int Count, decimal Percent, string Color);

    public record TopActiveUserDto(string Name, string Initials, int ActivityCount, decimal PercentOfMax);

    public record ActivityLogDashboardResponse(
        DateTime RangeStart,
        DateTime RangeEnd,
        int TotalActivities,
        decimal TotalActivitiesGrowthPercent,
        int BookActivities,
        decimal BookActivitiesGrowthPercent,
        int ReaderActivities,
        decimal ReaderActivitiesGrowthPercent,
        int ReviewActivities,
        decimal ReviewActivitiesGrowthPercent,
        int SystemActivities,
        decimal SystemActivitiesGrowthPercent,
        List<ActivitySummaryDto> ActivitySummary,
        List<TopActiveUserDto> TopActiveUsers,
        PagenatedList<AuditLogItemDto> Activities
    );
}