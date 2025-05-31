namespace Elearning.Modules.Program.Domain.Program;

public sealed class RecentActivity
{
    public Guid id { get; set; }
    public Guid user_id { get; set; }
    public string action { get; set; } = string.Empty; // 'assignment_created', 'assignment_submitted', etc.
    public string target_type { get; set; } = string.Empty; // 'assignment', 'quiz', 'lecture', 'course'
    public Guid target_id { get; set; }
    public string? target_title { get; set; }
    public Guid? course_id { get; set; }
    public string? course_name { get; set; }
    public string? metadata { get; set; } // JSON string for additional data
    public DateTime created_at { get; set; }

    public static RecentActivity Create(
        Guid userId,
        string action,
        string targetType,
        Guid targetId,
        string? targetTitle = null,
        Guid? courseId = null,
        string? courseName = null,
        object? metadata = null)
    {
        return new RecentActivity
        {
            id = Guid.NewGuid(),
            user_id = userId,
            action = action,
            target_type = targetType,
            target_id = targetId,
            target_title = targetTitle,
            course_id = courseId,
            course_name = courseName,
            metadata = metadata != null ? System.Text.Json.JsonSerializer.Serialize(metadata) : null,
            created_at = DateTime.UtcNow
        };
    }
}

public static class ActivityActions
{
    public const string AssignmentCreated = "assignment_created";
    public const string AssignmentSubmitted = "assignment_submitted";
    public const string AssignmentGraded = "assignment_graded";
    public const string QuizCompleted = "quiz_completed";
    public const string QuizStarted = "quiz_started";
    public const string LectureCompleted = "lecture_completed";
    public const string CourseEnrolled = "course_enrolled";
    public const string CourseCompleted = "course_completed";
    public const string NotificationReceived = "notification_received";
}

public static class TargetTypes
{
    public const string Assignment = "assignment";
    public const string Quiz = "quiz";
    public const string Lecture = "lecture";
    public const string Course = "course";
    public const string Notification = "notification";
}
