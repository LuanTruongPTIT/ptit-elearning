namespace Elearning.Modules.Program.Application.Program.GetClassStudents;

public sealed record GetClassStudentsResponse
{
  public List<ClassStudentDetail> Students { get; init; } = new();
  public int TotalCount { get; init; }
  public int Page { get; init; }
  public int PageSize { get; init; }
  public int TotalPages { get; init; }
  public bool HasNextPage { get; init; }
  public bool HasPreviousPage { get; init; }
}

public sealed record ClassStudentDetail
{
  public string StudentId { get; init; } = string.Empty;
  public string StudentName { get; init; } = string.Empty;
  public string Email { get; init; } = string.Empty;
  public string AvatarUrl { get; init; } = string.Empty;
  public double OverallProgress { get; init; }
  public int CompletedCourses { get; init; }
  public int InProgressCourses { get; init; }
  public int NotStartedCourses { get; init; }
  public int TotalAssignments { get; init; }
  public int CompletedAssignments { get; init; }
  public int PendingAssignments { get; init; }
  public double AverageGrade { get; init; }
  public DateTime LastAccessed { get; init; }
  public string Status { get; init; } = string.Empty; // active, inactive
  public List<StudentCourseProgress> CourseProgress { get; init; } = new();
}

public sealed record StudentCourseProgress
{
  public string CourseId { get; init; } = string.Empty;
  public string CourseName { get; init; } = string.Empty;
  public double Progress { get; init; }
  public string Status { get; init; } = string.Empty;
  public DateTime LastAccessed { get; init; }
}
