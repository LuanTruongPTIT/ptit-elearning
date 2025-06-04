using Elearning.Common.Application.Messaging;

namespace Elearning.Modules.Program.Application.Program.GetStudentDetail;

public sealed record GetStudentDetailQuery(string TeacherId, string StudentId) : IQuery<GetStudentDetailResponse>;

public sealed record GetStudentDetailResponse(
    string StudentId,
    string StudentName,
    string Email,
    string? AvatarUrl,
    string? PhoneNumber,
    string? Address,
    DateTime? DateOfBirth,
    string? Gender,
    DateTime EnrollmentDate,
    string ProgramName,
    string? Department,
    decimal OverallProgress,
    int CompletedCourses,
    int InProgressCourses,
    int NotStartedCourses,
    int TotalAssignments,
    int CompletedAssignments,
    int PendingAssignments,
    decimal AverageGrade,
    DateTime LastAccessed,
    string Status,
    List<StudentCourseDetailResponse> CourseProgress,
    List<StudentAssignmentDetailResponse> RecentAssignments,
    List<StudentActivityDetailResponse> RecentActivities
);

public sealed record StudentCourseDetailResponse(
    string CourseId,
    string CourseName,
    decimal Progress,
    string Status,
    DateTime? LastAccessed,
    int CompletedLectures,
    int TotalLectures,
    int CompletedAssignments,
    int TotalAssignments,
    decimal? CurrentGrade
);

public sealed record StudentAssignmentDetailResponse(
    string AssignmentId,
    string AssignmentTitle,
    string CourseName,
    DateTime Deadline,
    DateTime? SubmittedAt,
    decimal? Score,
    decimal MaxScore,
    string Status,
    bool IsLate
);

public sealed record StudentActivityDetailResponse(
    string ActivityType,
    string Description,
    DateTime Timestamp,
    string? CourseName,
    decimal? Score
);
