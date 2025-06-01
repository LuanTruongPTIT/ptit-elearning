using Elearning.Common.Domain;

namespace Elearning.Modules.Program.Domain.Program;

public sealed class AssignmentUpdatedDomainEvent : IDomainEvent
{
  public Guid Id { get; }
  public DateTime OccurredOnUtc { get; }
  public Guid AssignmentId { get; }
  public Guid TeachingAssignCourseId { get; }
  public string Title { get; }
  public DateTime Deadline { get; }

  public AssignmentUpdatedDomainEvent(
      Guid assignmentId,
      Guid teachingAssignCourseId,
      string title,
      DateTime deadline)
  {
    Id = Guid.NewGuid();
    OccurredOnUtc = DateTime.UtcNow;
    AssignmentId = assignmentId;
    TeachingAssignCourseId = teachingAssignCourseId;
    Title = title;
    Deadline = deadline;
  }
}
