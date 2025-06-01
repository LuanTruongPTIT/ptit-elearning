using Elearning.Common.Application.Messaging;

namespace Elearning.Modules.Program.Application.Program.GetAssignmentDetailsForTeacher;

public sealed record GetAssignmentDetailsForTeacherQuery(
    Guid AssignmentId,
    Guid TeacherId
) : IQuery<GetAssignmentDetailsForTeacherResponse>;

public sealed record GetAssignmentDetailsForTeacherResponse(
    Guid Id,
    string Title,
    string Description,
    DateTime Deadline,
    string AssignmentType,
    bool ShowAnswers,
    int? TimeLimitMinutes,
    List<string>? AttachmentUrls,
    decimal MaxScore,
    bool IsPublished,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    string CourseName,
    string ClassName,
    int TotalStudents,
    int SubmissionsCount,
    int GradedSubmissions,
    int PendingSubmissions,
    decimal? AverageGrade,
    List<RecentSubmissionInfo> RecentSubmissions
);

public sealed record RecentSubmissionInfo(
    Guid Id,
    string StudentName,
    string StudentEmail,
    DateTime SubmittedAt,
    decimal? Grade,
    string Status,
    bool IsLate
);
