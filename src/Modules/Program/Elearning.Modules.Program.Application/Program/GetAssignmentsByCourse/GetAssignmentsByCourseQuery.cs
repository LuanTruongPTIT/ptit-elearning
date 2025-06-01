using Elearning.Common.Application.Messaging;

namespace Elearning.Modules.Program.Application.Program.GetAssignmentsByCourse;

public sealed record GetAssignmentsByCourseQuery(
    Guid CourseId,
    Guid TeacherId
) : IQuery<List<GetAssignmentsByCourseResponse>>;

public sealed record GetAssignmentsByCourseResponse(
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
    Guid CreatedBy,
    int SubmissionsCount,
    int TotalStudents
);
