using Elearning.Common.Domain;

namespace Elearning.Modules.Program.Domain.Program;

public class Assignment : Entity
{
  public Guid id { get; private set; }
  public Guid teaching_assign_course_id { get; private set; }
  public string title { get; private set; }
  public string description { get; private set; }
  public DateTime deadline { get; private set; }
  public string assignment_type { get; private set; }
  public bool show_answers { get; private set; }
  public int? time_limit_minutes { get; private set; }
  public List<string>? attachment_urls { get; private set; }
  public decimal max_score { get; private set; }
  public bool is_published { get; private set; }
  public DateTime created_at { get; private set; }
  public DateTime updated_at { get; private set; }
  public Guid created_by { get; private set; }
  public Guid? updated_by { get; private set; }

  private Assignment() { }

  public static Assignment Create(
      Guid courseId,
      string title,
      string description,
      DateTime deadline,
      string assignmentType,
      bool showAnswers,
      int? timeLimit,
      List<string>? attachments,
      decimal maxScore,
      bool isPublished,
      Guid createdBy)
  {
    var assignment = new Assignment
    {
      id = Guid.NewGuid(),
      teaching_assign_course_id = courseId,
      title = title,
      description = description,
      deadline = deadline,
      assignment_type = assignmentType,
      show_answers = showAnswers,
      time_limit_minutes = timeLimit,
      attachment_urls = attachments,
      max_score = maxScore,
      is_published = isPublished,
      created_at = DateTime.UtcNow,
      updated_at = DateTime.UtcNow,
      created_by = createdBy
    };

    // Raise domain event for assignment creation
    assignment.Raise(new AssignmentCreatedDomainEvent(
        assignment.id,
        assignment.teaching_assign_course_id,
        assignment.title,
        assignment.description,
        assignment.deadline,
        assignment.created_by
    ));

    return assignment;
  }

  public void Update(
      string title,
      string? description,
      DateTime deadline,
      string assignmentType,
      bool showAnswers,
      int? timeLimitMinutes,
      List<string>? attachmentUrls,
      decimal maxScore,
      bool isPublished)
  {
    this.title = title;
    this.description = description;
    this.deadline = deadline;
    this.assignment_type = assignmentType;
    this.show_answers = showAnswers;
    this.time_limit_minutes = timeLimitMinutes;
    this.attachment_urls = attachmentUrls;
    this.max_score = maxScore;
    this.is_published = isPublished;
    this.updated_at = DateTime.UtcNow;

    // Raise domain event for update notification
    this.Raise(new AssignmentUpdatedDomainEvent(
        this.id,
        this.teaching_assign_course_id,
        this.title,
        this.deadline
    ));
  }
}
