namespace Elearning.Modules.Program.Application.Program.GetNotifications;

public sealed record GetNotificationsResponse(
    List<NotificationDto> notifications,
    int total_count,
    int unread_count,
    bool has_more
);

public sealed record NotificationDto(
    Guid id,
    Guid user_id,
    string title,
    string message,
    string type,
    string? target_type,
    Guid? target_id,
    bool is_read,
    string priority,
    DateTime created_at,
    DateTime? read_at,
    string time_ago
);
