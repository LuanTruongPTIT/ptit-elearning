using Elearning.Common.Application.Messaging;

namespace Elearning.Modules.Program.Application.Courses.Queries.GetCourseById;

public sealed record GetCourseByIdQuery(string CourseId) : IQuery<GetCourseByIdResponse>;

public sealed record GetCourseByIdResponse
{
  public string Id { get; init; } = string.Empty;
  public string Name { get; init; } = string.Empty;
  public string Code { get; init; } = string.Empty;
  public string Description { get; init; } = string.Empty;
  public string ThumbnailUrl { get; init; } = string.Empty;
  public string Status { get; init; } = string.Empty;
  public string CreatedAt { get; init; } = string.Empty;
  public string UpdatedAt { get; init; } = string.Empty;
  public string Department { get; init; } = string.Empty;
  public string TeacherName { get; init; } = string.Empty;
  public string TeacherAvatar { get; init; } = string.Empty;
  public int StudentsEnrolled { get; init; }
  public int MaxStudents { get; init; }
  public decimal CompletionRate { get; init; }
  public decimal Rating { get; init; }
  public int Duration { get; init; } // in hours

  // Statistics
  public CoursePerformanceStats PerformanceStats { get; init; } = new();
  public List<EnrolledStudent> EnrolledStudents { get; init; } = new();
  public List<MonthlyEnrollment> EnrollmentTrends { get; init; } = new();
  public List<ProgressDistribution> ProgressDistributions { get; init; } = new();
  public List<CourseContent> CourseContents { get; init; } = new();
}

public sealed record CoursePerformanceStats
{
  public decimal AverageScore { get; init; }
  public decimal CompletionRate { get; init; }
  public decimal DropoutRate { get; init; }
  public int TotalLessons { get; init; }
  public int CompletedLessons { get; init; }
  public int TotalAssignments { get; init; }
  public decimal AssignmentCompletionRate { get; init; }
  public int AverageStudyTime { get; init; } // in hours
  public string DifficultyLevel { get; init; } = string.Empty;
}

public sealed record EnrolledStudent
{
  public string StudentId { get; init; } = string.Empty;
  public string StudentName { get; init; } = string.Empty;
  public string Avatar { get; init; } = string.Empty;
  public decimal Progress { get; init; }
  public decimal Score { get; init; }
  public string Status { get; init; } = string.Empty;
  public string LastAccessed { get; init; } = string.Empty;
  public string EnrollmentDate { get; init; } = string.Empty;
}

public sealed record MonthlyEnrollment
{
  public string Month { get; init; } = string.Empty;
  public int NewEnrollments { get; init; }
  public int Completions { get; init; }
  public int Dropouts { get; init; }
  public decimal AverageScore { get; init; }
}

public sealed record ProgressDistribution
{
  public string Range { get; init; } = string.Empty;
  public int Count { get; init; }
  public decimal Percentage { get; init; }
  public string Color { get; init; } = string.Empty;
}

public sealed record CourseContent
{
  public string Id { get; init; } = string.Empty;
  public string Title { get; init; } = string.Empty;
  public string Type { get; init; } = string.Empty; // lesson, assignment, quiz
  public int Duration { get; init; } // in minutes
  public decimal CompletionRate { get; init; }
  public string Status { get; init; } = string.Empty;
  public int Order { get; init; }
}
