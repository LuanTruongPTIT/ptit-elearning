namespace Elearning.Modules.Program.Domain.Program;

public sealed class Notification
{
    public Guid id { get; set; }
    public Guid user_id { get; set; }
    public string title { get; set; } = string.Empty;
    public string message { get; set; } = string.Empty;
    public string type { get; set; } = string.Empty; // 'assignment', 'quiz', 'announcement', 'grade'
    public string? target_type { get; set; } // 'assignment', 'quiz', 'lecture', 'course'
    public Guid? target_id { get; set; }
    public bool is_read { get; set; } = false;
    public string priority { get; set; } = "normal"; // 'low', 'normal', 'high', 'urgent'
    public DateTime created_at { get; set; }
    public DateTime? read_at { get; set; }

    public static Notification Create(
        Guid userId,
        string title,
        string message,
        string type,
        string? targetType = null,
        Guid? targetId = null,
        string priority = "normal")
    {
        return new Notification
        {
            id = Guid.NewGuid(),
            user_id = userId,
            title = title,
            message = message,
            type = type,
            target_type = targetType,
            target_id = targetId,
            priority = priority,
            created_at = DateTime.UtcNow
        };
    }

    public void MarkAsRead()
    {
        is_read = true;
        read_at = DateTime.UtcNow;
    }
}

public static class NotificationTypes
{
    public const string Assignment = "assignment";
    public const string Quiz = "quiz";
    public const string Announcement = "announcement";
    public const string Grade = "grade";
    public const string Reminder = "reminder";
    public const string System = "system";
}

public static class NotificationPriorities
{
    public const string Low = "low";
    public const string Normal = "normal";
    public const string High = "high";
    public const string Urgent = "urgent";
}
