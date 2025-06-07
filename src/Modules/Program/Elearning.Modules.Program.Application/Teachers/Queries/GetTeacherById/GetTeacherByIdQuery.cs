using Elearning.Common.Application.Messaging;

namespace Elearning.Modules.Program.Application.Teachers.Queries.GetTeacherById;

public sealed record GetTeacherByIdQuery(string TeacherId) : IQuery<GetTeacherByIdResponse>;

public sealed record GetTeacherByIdResponse
{
    public string Id { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string PhoneNumber { get; init; } = string.Empty;
    public string Department { get; init; } = string.Empty;
    public string JoinDate { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public string Avatar { get; init; } = string.Empty;
    public string Address { get; init; } = string.Empty;
    public string Specialization { get; init; } = string.Empty;
    public decimal Rating { get; init; }
    public int CoursesCount { get; init; }
    public int StudentsCount { get; init; }

    // Statistics
    public TeacherPerformanceStats PerformanceStats { get; init; } = new();
    public List<TeachingCourse> TeachingCourses { get; init; } = new();
    public List<MonthlyTeachingActivity> TeachingActivities { get; init; } = new();
    public List<StudentPerformanceByTeacher> StudentPerformances { get; init; } = new();
    public List<DepartmentComparison> DepartmentComparisons { get; init; } = new();
}

public sealed record TeacherPerformanceStats
{
    public decimal AverageStudentScore { get; init; }
    public decimal StudentSatisfactionRate { get; init; }
    public int TotalLessons { get; init; }
    public int CompletedLessons { get; init; }
    public int TotalAssignments { get; init; }
    public double AssignmentCompletionRate { get; init; }
    public int TeachingHours { get; init; }
    public string Rank { get; init; } = string.Empty;
}

public sealed record TeachingCourse
{
    public string CourseId { get; init; } = string.Empty;
    public string CourseName { get; init; } = string.Empty;
    public string CourseCode { get; init; } = string.Empty;
    public int StudentsEnrolled { get; init; }
    public int MaxStudents { get; init; }
    public decimal CompletionRate { get; init; }
    public decimal AverageScore { get; init; }
    public string Status { get; init; } = string.Empty;
    public string StartDate { get; init; } = string.Empty;
}

public sealed record MonthlyTeachingActivity
{
    public string Month { get; init; } = string.Empty;
    public int TeachingHours { get; init; }
    public int StudentsGraded { get; init; }
    public int AssignmentsCreated { get; init; }
    public decimal AverageStudentScore { get; init; }
}

public sealed record StudentPerformanceByTeacher
{
    public string StudentName { get; init; } = string.Empty;
    public string CourseName { get; init; } = string.Empty;
    public decimal Score { get; init; }
    public decimal Progress { get; init; }
    public string Status { get; init; } = string.Empty;
}

public sealed record DepartmentComparison
{
    public string Metric { get; init; } = string.Empty;
    public decimal TeacherValue { get; init; }
    public decimal DepartmentAverage { get; init; }
    public string Trend { get; init; } = string.Empty;
}
