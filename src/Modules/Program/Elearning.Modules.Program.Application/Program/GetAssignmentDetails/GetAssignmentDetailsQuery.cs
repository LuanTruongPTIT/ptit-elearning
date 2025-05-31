using Elearning.Common.Application.Messaging;

namespace Elearning.Modules.Program.Application.Program.GetAssignmentDetails;

public sealed record GetAssignmentDetailsQuery(
    Guid AssignmentId,
    Guid StudentId
) : IQuery<GetAssignmentDetailsResponse>;

public sealed record GetAssignmentDetailsResponse(
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
    string CourseName,
    string TeacherName,
    bool HasSubmission,
    AssignmentSubmissionInfo? Submission
);

public sealed record AssignmentSubmissionInfo(
    Guid Id,
    string SubmissionType,
    List<string>? FileUrls,
    DateTime SubmittedAt,
    decimal? Grade,
    string? Feedback,
    string Status
);
