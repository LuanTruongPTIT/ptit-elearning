using Elearning.Common.Domain;

namespace Elearning.Modules.Program.Domain.Program;

public sealed class AssignmentCreatedDomainEvent : IDomainEvent
{
  public Guid Id { get; }
  public DateTime OccurredOnUtc { get; }
  public Guid AssignmentId { get; }
  public Guid TeachingAssignCourseId { get; }
  public string Title { get; }
  public string? Description { get; }
  public DateTime Deadline { get; }
  public Guid CreatedBy { get; }

  public AssignmentCreatedDomainEvent(
      Guid assignmentId,
      Guid teachingAssignCourseId,
      string title,
      string? description,
      DateTime deadline,
      Guid createdBy)
  {
    Id = Guid.NewGuid();
    OccurredOnUtc = DateTime.UtcNow;
    AssignmentId = assignmentId;
    TeachingAssignCourseId = teachingAssignCourseId;
    Title = title;
    Description = description;
    Deadline = deadline;
    CreatedBy = createdBy;
  }
}
