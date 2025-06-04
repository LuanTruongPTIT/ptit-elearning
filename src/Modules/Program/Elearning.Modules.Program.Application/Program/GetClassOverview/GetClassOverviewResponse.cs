namespace Elearning.Modules.Program.Application.Program.GetClassOverview;

public sealed record GetClassOverviewResponse
{
  public string ClassId { get; init; } = string.Empty;
  public string ClassName { get; init; } = string.Empty;
  public string ProgramName { get; init; } = string.Empty;
  public int TotalStudents { get; init; }
  public int ActiveStudents { get; init; }
  public int InactiveStudents { get; init; }
  public double AverageProgress { get; init; }
  public int TotalCourses { get; init; }
  public int CompletedAssignments { get; init; }
  public int PendingAssignments { get; init; }
  public List<CourseProgressSummary> CourseProgress { get; init; } = new();
  public List<StudentPerformanceSummary> TopPerformers { get; init; } = new();
  public List<StudentPerformanceSummary> LowPerformers { get; init; } = new();
}

public sealed record CourseProgressSummary
{
  public string CourseId { get; init; } = string.Empty;
  public string CourseName { get; init; } = string.Empty;
  public double AverageProgress { get; init; }
  public int StudentsCompleted { get; init; }
  public int StudentsInProgress { get; init; }
  public int StudentsNotStarted { get; init; }
}

public sealed record StudentPerformanceSummary
{
  public string StudentId { get; init; } = string.Empty;
  public string StudentName { get; init; } = string.Empty;
  public string Email { get; init; } = string.Empty;
  public double OverallProgress { get; init; }
  public int CompletedCourses { get; init; }
  public int InProgressCourses { get; init; }
  public DateTime LastAccessed { get; init; }
}
