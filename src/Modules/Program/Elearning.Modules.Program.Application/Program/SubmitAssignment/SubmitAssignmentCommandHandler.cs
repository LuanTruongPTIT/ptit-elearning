using System.Data.Common;
using System.Text.Json;
using Dapper;
using Elearning.Common.Application.Data;
using Elearning.Common.Application.Messaging;
using Elearning.Common.Domain;

namespace Elearning.Modules.Program.Application.Program.SubmitAssignment;

internal sealed class SubmitAssignmentCommandHandler : ICommandHandler<SubmitAssignmentCommand, SubmitAssignmentResponse>
{
  private readonly IDbConnectionFactory _dbConnectionFactory;

  public SubmitAssignmentCommandHandler(IDbConnectionFactory dbConnectionFactory)
  {
    _dbConnectionFactory = dbConnectionFactory;
  }

  public async Task<Result<SubmitAssignmentResponse>> Handle(SubmitAssignmentCommand request, CancellationToken cancellationToken)
  {
    await using DbConnection connection = await _dbConnectionFactory.OpenConnectionAsync();
    using var transaction = await connection.BeginTransactionAsync(cancellationToken);

    try
    {
      // Validate assignment exists and is published
      const string validateAssignmentSql = @"
                SELECT id, deadline, assignment_type, is_published
                FROM programs.table_assignments
                WHERE id = @AssignmentId AND is_published = true";

      var assignment = await connection.QueryFirstOrDefaultAsync(validateAssignmentSql, new
      {
        AssignmentId = request.AssignmentId
      }, transaction);

      if (assignment == null)
      {
        return Result.Failure<SubmitAssignmentResponse>(
            Error.NotFound("Assignment.NotFound", "Assignment not found or not published"));
      }

      // Check if deadline has passed
      if (assignment.deadline < DateTime.UtcNow)
      {
        return Result.Failure<SubmitAssignmentResponse>(
            Error.Validation("Assignment.Deadline", "Assignment deadline has passed"));
      }

      // Check if student already has a submission
      const string checkSubmissionSql = @"
                SELECT id FROM programs.table_assignment_submissions
                WHERE assignment_id = @AssignmentId AND student_id = @StudentId";

      var existingSubmission = await connection.QueryFirstOrDefaultAsync<Guid?>(checkSubmissionSql, new
      {
        AssignmentId = request.AssignmentId,
        StudentId = request.StudentId
      }, transaction);

      var submissionId = existingSubmission ?? Guid.NewGuid();
      var submittedAt = DateTime.UtcNow;

      // Prepare file URLs as string array for PostgreSQL text[] type
      string[]? fileUrlsArray = request.FileUrls?.ToArray();

      // Use TextContent if provided, otherwise use SubmissionType
      string submissionText = !string.IsNullOrEmpty(request.TextContent)
        ? request.TextContent
        : request.SubmissionType;

      if (existingSubmission.HasValue)
      {
        // Update existing submission
        const string updateSubmissionSql = @"
                    UPDATE programs.table_assignment_submissions
                    SET submission_text = @SubmissionText,
                        file_attachments = @FileAttachments,
                        submitted_at = @SubmittedAt
                    WHERE id = @SubmissionId";

        await connection.ExecuteAsync(updateSubmissionSql, new
        {
          SubmissionId = submissionId,
          SubmissionText = submissionText,
          FileAttachments = fileUrlsArray,
          SubmittedAt = submittedAt
        }, transaction);
      }
      else
      {
        // Create new submission
        const string insertSubmissionSql = @"
                    INSERT INTO programs.table_assignment_submissions (
                        id, assignment_id, student_id, submission_text, file_attachments,
                        submitted_at
                    ) VALUES (
                        @Id, @AssignmentId, @StudentId, @SubmissionText, @FileAttachments,
                        @SubmittedAt
                    )";

        await connection.ExecuteAsync(insertSubmissionSql, new
        {
          Id = submissionId,
          AssignmentId = request.AssignmentId,
          StudentId = request.StudentId,
          SubmissionText = submissionText,
          FileAttachments = fileUrlsArray,
          SubmittedAt = submittedAt
        }, transaction);
      }

      await transaction.CommitAsync(cancellationToken);

      return Result.Success(new SubmitAssignmentResponse(
          submissionId,
          existingSubmission.HasValue ? "Assignment updated successfully" : "Assignment submitted successfully",
          submittedAt
      ));
    }
    catch (Exception ex)
    {
      Console.WriteLine(ex);
      await transaction.RollbackAsync(cancellationToken);
      return Result.Failure<SubmitAssignmentResponse>(
          Error.Failure("SubmitAssignment.Failed", ex.Message));
    }
  }
}
