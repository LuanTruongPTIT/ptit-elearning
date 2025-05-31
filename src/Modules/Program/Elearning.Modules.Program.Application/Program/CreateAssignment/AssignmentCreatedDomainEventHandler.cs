using System.Data.Common;
using System.Text.Json;
using Dapper;
using Elearning.Common.Application.Data;
using Elearning.Common.Application.EventBus;
using Elearning.Common.Application.Messaging;
using Elearning.Modules.Program.Domain.Program;

namespace Elearning.Modules.Program.Application.Program.CreateAssignment;

internal sealed class AssignmentCreatedDomainEventHandler : DomainEventHandler<AssignmentCreatedDomainEvent>
{
  private readonly IDbConnectionFactory _dbConnectionFactory;
  private readonly IEventBus _eventBus;

  public AssignmentCreatedDomainEventHandler(
      IDbConnectionFactory dbConnectionFactory,
      IEventBus eventBus)
  {
    _dbConnectionFactory = dbConnectionFactory;
    _eventBus = eventBus;
  }

  public override async Task Handle(AssignmentCreatedDomainEvent domainEvent, CancellationToken cancellationToken = default)
  {
    await using DbConnection connection = await _dbConnectionFactory.OpenConnectionAsync();

    // 1. Get course information and enrolled students
    const string getCourseAndStudentsSql = @"
            SELECT
                tac.course_class_name,
                tac.id as course_id,
                ce.student_id,
                u.email,
                u.full_name
            FROM programs.table_teaching_assign_courses tac
            INNER JOIN programs.table_course_enrollments ce ON ce.course_id = tac.id
            LEFT JOIN users.table_users u ON u.id = ce.student_id
            WHERE tac.id = @teaching_assign_course_id
            AND ce.status = 'active'";

    var courseAndStudents = await connection.QueryAsync(getCourseAndStudentsSql, new
    {
      teaching_assign_course_id = domainEvent.TeachingAssignCourseId
    });

    var courseInfo = courseAndStudents.FirstOrDefault();
    if (courseInfo == null) return;

    var students = courseAndStudents.Where(x => x.student_id != null).ToList();

    // 2. Create recent activity for teacher (assignment creator)
    var teacherActivity = RecentActivity.Create(
        domainEvent.CreatedBy,
        ActivityActions.AssignmentCreated,
        TargetTypes.Assignment,
        domainEvent.AssignmentId,
        domainEvent.Title,
        domainEvent.TeachingAssignCourseId,
        courseInfo.course_class_name,
        new { deadline = domainEvent.Deadline }
    );

    const string insertActivitySql = @"
            INSERT INTO programs.table_recent_activities (
                id, user_id, action, target_type, target_id, target_title,
                course_id, course_name, metadata, created_at
            ) VALUES (
                @id, @user_id, @action, @target_type, @target_id, @target_title,
                @course_id, @course_name, @metadata, @created_at
            )";

    await connection.ExecuteAsync(insertActivitySql, (object)teacherActivity);

    // 3. Create outbox notifications for students
    foreach (var student in students)
    {
      // Create notification content as JSON
      var notificationContent = new
      {
        StudentId = student.student_id.ToString(),
        AssignmentId = domainEvent.AssignmentId.ToString(),
        CourseId = domainEvent.TeachingAssignCourseId.ToString(),
        Title = domainEvent.Title,
        Message = $"Bài tập '{domainEvent.Title}' đã được tạo trong lớp {courseInfo.course_class_name}. Hạn nộp: {domainEvent.Deadline:dd/MM/yyyy HH:mm}",
        Deadline = domainEvent.Deadline,
        AssignmentType = "assignment", // You can get this from domainEvent if available
        NotificationType = "assignment_created"
      };

      const string insertOutboxSql = @"
                INSERT INTO users.table_outbox_messages (
                    id, type, content, occurred_on_utc, is_processed, retry_count
                ) VALUES (
                    @id, @type, @content, @occurred_on_utc, @is_processed, @retry_count
                )";

      await connection.ExecuteAsync(insertOutboxSql, new
      {
        id = Guid.NewGuid(),
        type = "AssignmentCreatedNotification",
        content = JsonSerializer.Serialize(notificationContent),
        occurred_on_utc = domainEvent.OccurredOnUtc,
        is_processed = false,
        retry_count = 0
      });

      // Create recent activity for student
      var studentActivity = RecentActivity.Create(
          (Guid)student.student_id,
          ActivityActions.NotificationReceived,
          TargetTypes.Assignment,
          domainEvent.AssignmentId,
          domainEvent.Title,
          domainEvent.TeachingAssignCourseId,
          courseInfo.course_class_name,
          new
          {
            deadline = domainEvent.Deadline,
            notification_type = "assignment_created"
          }
      );

      await connection.ExecuteAsync(insertActivitySql, (object)studentActivity);
    }

    // 4. Publish integration event for real-time notifications
    var integrationEvent = new AssignmentCreatedIntegrationEvent(
        domainEvent.Id,
        domainEvent.OccurredOnUtc,
        domainEvent.AssignmentId,
        domainEvent.TeachingAssignCourseId,
        domainEvent.Title,
        domainEvent.Description,
        domainEvent.Deadline,
        students.Select(s => (Guid)s.student_id).ToList()
    );

    await _eventBus.PublishAsync(integrationEvent, cancellationToken);
  }
}
