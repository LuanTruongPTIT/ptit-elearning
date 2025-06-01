using System.Data.Common;
using Dapper;
using Elearning.Common.Application.Data;
using Elearning.Common.Application.Messaging;
using Elearning.Common.Domain;

namespace Elearning.Modules.Program.Application.Program.GradeAssignment;

internal sealed class GradeAssignmentCommandHandler(
    IDbConnectionFactory dbConnectionFactory) : ICommandHandler<GradeAssignmentCommand, GradeAssignmentResponse>
{
  public async Task<Result<GradeAssignmentResponse>> Handle(GradeAssignmentCommand request, CancellationToken cancellationToken)
  {
    try
    {
      await using DbConnection connection = await dbConnectionFactory.OpenConnectionAsync();

      // Verify teacher has access to this submission
      var accessQuery = @"
                SELECT COUNT(*)
                FROM programs.table_assignment_submissions asub
                INNER JOIN programs.table_assignments a ON asub.assignment_id = a.id
                INNER JOIN programs.table_teaching_assign_courses tac ON a.course_id = tac.id
                WHERE asub.id = @SubmissionId AND tac.teacher_id = @TeacherId";

      var hasAccess = await connection.QuerySingleAsync<int>(accessQuery, new { request.SubmissionId, request.TeacherId });

      if (hasAccess == 0)
      {
        return Result.Failure<GradeAssignmentResponse>(
            new Error("GradeAssignment.Unauthorized", "You don't have access to this submission", ErrorType.Authorization));
      }

      // Get assignment max score for validation
      var maxScoreQuery = @"
                SELECT a.max_score
                FROM programs.table_assignment_submissions asub
                INNER JOIN programs.table_assignments a ON asub.assignment_id = a.id
                WHERE asub.id = @SubmissionId";

      var maxScore = await connection.QuerySingleAsync<decimal>(maxScoreQuery, new { request.SubmissionId });

      if (request.Grade < 0 || request.Grade > maxScore)
      {
        return Result.Failure<GradeAssignmentResponse>(
            new Error("GradeAssignment.InvalidGrade", $"Grade must be between 0 and {maxScore}", ErrorType.Validation));
      }

      // Update submission with grade and feedback
      var updateQuery = @"
                UPDATE programs.table_assignment_submissions 
                SET 
                    grade = @Grade,
                    feedback = @Feedback,
                    status = 'graded',
                    updated_at = CURRENT_TIMESTAMP
                WHERE id = @SubmissionId
                RETURNING grade, feedback, updated_at, status";

      var result = await connection.QuerySingleAsync<dynamic>(updateQuery, new
      {
        request.SubmissionId,
        request.Grade,
        request.Feedback
      });

      // Create notification for student
      var notificationQuery = @"
                INSERT INTO programs.table_notifications (
                    id, student_id, assignment_id, type, title, message, created_at
                )
                SELECT 
                    gen_random_uuid(),
                    asub.student_id,
                    asub.assignment_id,
                    'assignment_graded',
                    'Bài tập đã được chấm điểm',
                    CONCAT('Bài tập ""', a.title, '"" đã được chấm điểm: ', @Grade, '/', a.max_score),
                    CURRENT_TIMESTAMP
                FROM programs.table_assignment_submissions asub
                INNER JOIN programs.table_assignments a ON asub.assignment_id = a.id
                WHERE asub.id = @SubmissionId";

      await connection.ExecuteAsync(notificationQuery, new { request.SubmissionId, request.Grade });

      var response = new GradeAssignmentResponse
      {
        SubmissionId = request.SubmissionId,
        Grade = result.grade,
        Feedback = result.feedback,
        GradedAt = result.updated_at,
        Status = result.status
      };

      return Result.Success(response);
    }
    catch (Exception ex)
    {
      return Result.Failure<GradeAssignmentResponse>(
          new Error("GradeAssignment.DatabaseError", $"Database error: {ex.Message}", ErrorType.Failure));
    }
  }
}
