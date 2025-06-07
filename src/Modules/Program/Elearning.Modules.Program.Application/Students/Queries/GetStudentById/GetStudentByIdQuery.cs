using Elearning.Common.Application.Messaging;

namespace Elearning.Modules.Program.Application.Students.Queries.GetStudentById;

public sealed record GetStudentByIdQuery(string StudentId) : IQuery<GetStudentByIdResponse>;

public sealed record GetStudentByIdResponse
{
  public string Id { get; init; } = string.Empty;
  public string Name { get; init; } = string.Empty;
  public string Email { get; init; } = string.Empty;
  public string PhoneNumber { get; init; } = string.Empty;
  public string DateOfBirth { get; init; } = string.Empty;
  public string EnrollmentDate { get; init; } = string.Empty;
  public string Status { get; init; } = string.Empty;
  public string Department { get; init; } = string.Empty;
  public string Program { get; init; } = string.Empty;
  public decimal Gpa { get; init; }
  public int TotalCourses { get; init; }
  public int CompletedCourses { get; init; }
  public int InProgressCourses { get; init; }
  public string Avatar { get; init; } = string.Empty;
  public string Address { get; init; } = string.Empty;

  // Statistics
  public StudentPerformanceStats PerformanceStats { get; init; } = new();
  public List<CourseProgress> CourseProgresses { get; init; } = new();
  public List<MonthlyActivity> StudyActivities { get; init; } = new();
  public List<SubjectScore> SubjectScores { get; init; } = new();
}

public sealed record StudentPerformanceStats
{
  public decimal AverageScore { get; init; }
  public decimal AttendanceRate { get; init; }
  public int TotalAssignments { get; init; }
  public int CompletedAssignments { get; init; }
  public int TotalQuizzes { get; init; }
  public decimal QuizAverageScore { get; init; }
  public int StudyHours { get; init; }
  public string Rank { get; init; } = string.Empty;
}

public sealed record CourseProgress
{
  public string CourseId { get; init; } = string.Empty;
  public string CourseName { get; init; } = string.Empty;
  public string CourseCode { get; init; } = string.Empty;
  public string TeacherName { get; init; } = string.Empty;
  public decimal ProgressPercentage { get; init; }
  public decimal CurrentScore { get; init; }
  public string Status { get; init; } = string.Empty;
  public string LastAccessed { get; init; } = string.Empty;
}

public sealed record MonthlyActivity
{
  public string Month { get; init; } = string.Empty;
  public int StudyHours { get; init; }
  public int AssignmentsCompleted { get; init; }
  public int QuizzesTaken { get; init; }
  public decimal AverageScore { get; init; }
}

public sealed record SubjectScore
{
  public string Subject { get; init; } = string.Empty;
  public decimal Score { get; init; }
  public string Grade { get; init; } = string.Empty;
  public string Color { get; init; } = string.Empty;
}
