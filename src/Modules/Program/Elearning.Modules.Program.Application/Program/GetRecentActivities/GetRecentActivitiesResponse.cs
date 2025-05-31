namespace Elearning.Modules.Program.Application.Program.GetRecentActivities;

public sealed record GetRecentActivitiesResponse(
    List<RecentActivityDto> activities,
    int total_count,
    bool has_more
);

public sealed record RecentActivityDto(
    Guid id,
    Guid user_id,
    string action,
    string target_type,
    Guid target_id,
    string? target_title,
    Guid? course_id,
    string? course_name,
    object? metadata,
    DateTime created_at,
    string time_ago
);
