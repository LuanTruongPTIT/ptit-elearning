using System.Data.Common;
using System.Text.Json;
using Dapper;
using Elearning.Common.Application.Data;
using Elearning.Common.Application.Messaging;
using Elearning.Common.Domain;
using Elearning.Modules.Program.Domain.Program;

namespace Elearning.Modules.Program.Application.Program.CreateAssignment;

internal sealed class CreateAssignmentCommandHandler : ICommandHandler<CreateAssignmentCommand, CreateAssignmentResponse>
{
    private readonly IDbConnectionFactory _dbConnectionFactory;

    public CreateAssignmentCommandHandler(IDbConnectionFactory dbConnectionFactory)
    {
        _dbConnectionFactory = dbConnectionFactory;
    }

    public async Task<Result<CreateAssignmentResponse>> Handle(CreateAssignmentCommand request, CancellationToken cancellationToken)
    {
        await using DbConnection connection = await _dbConnectionFactory.OpenConnectionAsync();
        using var transaction = await connection.BeginTransactionAsync(cancellationToken);

        try
        {
            // Validate teaching assignment course exists
            const string validateCourseSql = @"
                SELECT id FROM programs.table_teaching_assign_courses
                WHERE id = @course_id";

            var courseExists = await connection.QueryFirstOrDefaultAsync<Guid?>(
                validateCourseSql,
                new { course_id = request.course_id },
                transaction);

            if (!courseExists.HasValue)
            {
                return Result.Failure<CreateAssignmentResponse>(Error.Failure("Course.NotFound", "Teaching assignment course not found"));
            }

            // Create assignment domain object
            var assignment = Assignment.Create(
                request.course_id,
                request.title,
                request.description,
                request.deadline,
                request.assignment_type,
                request.show_answers,
                request.time_limit,
                request.attachments,
                request.max_score,
                request.is_published,
                request.created_by
            );

            // Insert assignment
            const string insertAssignmentSql = @"
                INSERT INTO programs.table_assignments (
                    id, teaching_assign_course_id, title, description, deadline,
                    assignment_type, show_answers, time_limit_minutes, attachment_urls,
                    max_score, is_published, created_at, updated_at, created_by
                ) VALUES (
                    @id, @teaching_assign_course_id, @title, @description, @deadline,
                    @assignment_type, @show_answers, @time_limit_minutes, @attachment_urls,
                    @max_score, @is_published, @created_at, @updated_at, @created_by
                )";

            await connection.ExecuteAsync(insertAssignmentSql, assignment, transaction);

            // Get students from the class assigned to this teaching assignment course
            const string getClassStudentsSql = @"
                SELECT DISTINCT sp.student_id 
                FROM programs.table_teaching_assign_courses tac
                INNER JOIN programs.classes c ON c.id = tac.class_id
                INNER JOIN programs.table_student_programs sp ON sp.program_id = c.program_id
                WHERE tac.id = @courseId";

            var enrolledStudents = await connection.QueryAsync<Guid>(
                getClassStudentsSql,
                new { courseId = request.course_id },
                transaction);
            Console.WriteLine($"CreateAssignmentCommandHandler: Enrolled students count: {enrolledStudents.Count()}");
            // Insert notifications for each enrolled student
            foreach (var studentId in enrolledStudents)
            {
                const string insertNotificationSql = @"
                    INSERT INTO programs.table_notifications (
                        id, user_id, title, message, type, target_type, target_id, is_read, created_at
                    ) VALUES (
                        @id, @user_id, @title, @message, @type, @target_type, @target_id, @is_read, @created_at
                    )";

                await connection.ExecuteAsync(insertNotificationSql, new
                {
                    id = Guid.NewGuid(),
                    user_id = studentId,
                    title = $"Bài tập mới: {assignment.title}",
                    message = $"Bài tập '{assignment.title}' đã được tạo. Hạn nộp: {assignment.deadline:dd/MM/yyyy HH:mm}",
                    type = "assignment_created",
                    target_type = "assignment",
                    target_id = assignment.id,
                    is_read = false,
                    created_at = DateTime.UtcNow
                }, transaction);
            }

            // Process other domain events if needed (keep outbox for other events)
            foreach (var domainEvent in assignment.DomainEvents)
            {
                // Skip AssignmentCreatedDomainEvent since we handle it directly above
                if (domainEvent.GetType().Name == "AssignmentCreatedDomainEvent")
                    continue;

                // Insert outbox message for other domain events
                const string insertOutboxSql = @"
                    INSERT INTO users.table_outbox_messages (id, type, content, occurred_on_utc)
                    VALUES (@id, @type, @content, @occurred_on_utc)";

                await connection.ExecuteAsync(insertOutboxSql, new
                {
                    id = domainEvent.Id,
                    type = domainEvent.GetType().Name,
                    content = JsonSerializer.Serialize(domainEvent),
                    occurred_on_utc = domainEvent.OccurredOnUtc
                }, transaction);
            }

            await transaction.CommitAsync(cancellationToken);
            return new CreateAssignmentResponse(assignment.id, "Assignment created successfully");
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            await transaction.RollbackAsync(cancellationToken);
            return Result.Failure<CreateAssignmentResponse>(Error.Failure("CreateAssignment.Failed", ex.Message));
        }
    }
}
