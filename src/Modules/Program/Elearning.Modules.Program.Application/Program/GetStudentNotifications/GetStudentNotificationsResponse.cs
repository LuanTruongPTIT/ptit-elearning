namespace Elearning.Modules.Program.Application.Program.GetStudentNotifications;

public class GetStudentNotificationsResponse
{
  public List<StudentNotificationDto> Notifications { get; set; } = new();
  public int TotalCount { get; set; }
  public int PageNumber { get; set; }
  public int PageSize { get; set; }
  public bool HasNextPage { get; set; }
}

public class StudentNotificationDto
{
  public string Id { get; set; } = string.Empty;
  public string Type { get; set; } = string.Empty;
  public string Title { get; set; } = string.Empty;
  public string Message { get; set; } = string.Empty;
  public string? AssignmentId { get; set; }
  public string? CourseId { get; set; }
  public string? CourseName { get; set; }
  public DateTime? Deadline { get; set; }
  public string? AssignmentType { get; set; }
  public DateTime CreatedAt { get; set; }
  public bool IsRead { get; set; } = false;
  public bool IsNew { get; set; } = false; // Created in last 24 hours
}
