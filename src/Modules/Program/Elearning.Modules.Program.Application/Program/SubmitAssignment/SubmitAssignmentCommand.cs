using Elearning.Common.Application.Messaging;

namespace Elearning.Modules.Program.Application.Program.SubmitAssignment;

public sealed record SubmitAssignmentCommand(
    Guid AssignmentId,
    Guid StudentId,
    string SubmissionType, // "file", "quiz", "both"
    List<string>? FileUrls,
    string? TextContent,
    Dictionary<string, object>? QuizAnswers
) : ICommand<SubmitAssignmentResponse>;

public sealed record SubmitAssignmentResponse(
    Guid SubmissionId,
    string Message,
    DateTime SubmittedAt
);
