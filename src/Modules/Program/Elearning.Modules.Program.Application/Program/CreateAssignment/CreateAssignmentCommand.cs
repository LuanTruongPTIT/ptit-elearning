using Elearning.Common.Application.Messaging;

namespace Elearning.Modules.Program.Application.Program.CreateAssignment;

public sealed record CreateAssignmentCommand(
    Guid course_id, // Changed from teaching_assign_course_id to match UI
    string title,
    string? description,
    DateTime deadline,
    string assignment_type, // 'upload', 'quiz', 'both'
    bool show_answers,
    int? time_limit, // Changed from time_limit_minutes to match UI
    List<string>? attachments, // Changed from attachment_urls to match UI
    decimal max_score,
    bool is_published
) : ICommand<CreateAssignmentResponse>
{
  public Guid created_by { get; set; }
}

public sealed record CreateAssignmentResponse(
    Guid assignment_id,
    string message
);
