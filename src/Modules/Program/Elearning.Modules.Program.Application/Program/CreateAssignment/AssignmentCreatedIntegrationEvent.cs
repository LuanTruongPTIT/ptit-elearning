using Elearning.Common.Application.EventBus;

namespace Elearning.Modules.Program.Application.Program.CreateAssignment;

public sealed class AssignmentCreatedIntegrationEvent : IntegrationEvent
{
  public Guid AssignmentId { get; }
  public Guid TeachingAssignCourseId { get; }
  public string Title { get; }
  public string? Description { get; }
  public DateTime Deadline { get; }
  public List<Guid> StudentIds { get; }

  public AssignmentCreatedIntegrationEvent(
      Guid id,
      DateTime occurredOnUtc,
      Guid assignmentId,
      Guid teachingAssignCourseId,
      string title,
      string? description,
      DateTime deadline,
      List<Guid> studentIds) : base(id, occurredOnUtc)
  {
    AssignmentId = assignmentId;
    TeachingAssignCourseId = teachingAssignCourseId;
    Title = title;
    Description = description;
    Deadline = deadline;
    StudentIds = studentIds;
  }
}
