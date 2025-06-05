using Elearning.Common.Application.Messaging;

namespace Elearning.Modules.Program.Application.Program.GetTeacherDashboard;

public sealed record GetTeacherDashboardQuery(string TeacherId) : IQuery<GetTeacherDashboardResponse>;

public sealed record GetTeacherDashboardResponse(
    int TotalStudents,
    int TotalCourses,
    int TotalClasses,
    decimal AverageCompletionRate,
    decimal AverageGrade,
    int CompletedAssignments,
    int PendingAssignments,
    int TotalAssignments,
    List<TeacherCourseOverview> CourseOverviews,
    List<TeacherRecentActivity> RecentActivities,
    List<TeacherClassSummary> ClassSummaries
);

public sealed record TeacherCourseOverview(
    Guid CourseId,
    string CourseName,
    string ClassName,
    int EnrolledStudents,
    decimal AverageProgress,
    int CompletedStudents,
    int InProgressStudents,
    int NotStartedStudents
);

public sealed record TeacherRecentActivity(
    string ActivityType,
    string Description,
    DateTime Timestamp,
    string StudentName,
    string CourseName,
    decimal? Score
);

public sealed record TeacherClassSummary(
    Guid ClassId,
    string ClassName,
    string ProgramName,
    int StudentCount,
    decimal AverageProgress,
    int CourseCount
);
