using Elearning.Common.Application.Messaging;

namespace Elearning.Modules.Program.Application.Program.GradeAssignment;

public sealed record GradeAssignmentCommand(
    Guid SubmissionId,
    Guid TeacherId,
    decimal Grade,
    string? Feedback = null
) : ICommand<GradeAssignmentResponse>;

public class GradeAssignmentResponse
{
  public Guid SubmissionId { get; set; }
  public decimal Grade { get; set; }
  public string? Feedback { get; set; }
  public DateTime GradedAt { get; set; }
  public string Status { get; set; } = string.Empty;
}
