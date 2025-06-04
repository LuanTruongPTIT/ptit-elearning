using Elearning.Common.Application.Messaging;

namespace Elearning.Modules.Program.Application.Program.GetTeacherClasses;

public sealed record GetTeacherClassesQuery(string TeacherId) : IQuery<List<GetTeacherClassesResponse>>;

public sealed record GetTeacherClassesResponse(
    Guid ClassId,
    string ClassName,
    string ProgramName,
    int TotalStudents,
    int ActiveStudents,
    int InactiveStudents,
    decimal AverageProgress,
    int TotalCourses,
    int CompletedAssignments,
    int PendingAssignments,
    List<CourseProgressResponse> CourseProgress,
    List<StudentPerformanceResponse> TopPerformers,
    List<StudentPerformanceResponse> LowPerformers
);

public sealed record CourseProgressResponse(
    Guid CourseId,
    string CourseName,
    decimal AverageProgress,
    int StudentsCompleted,
    int StudentsInProgress,
    int StudentsNotStarted
);

public sealed record StudentPerformanceResponse(
    Guid StudentId,
    string StudentName,
    string Email,
    decimal OverallProgress,
    int CompletedCourses,
    int InProgressCourses,
    DateTime LastAccessed
);
