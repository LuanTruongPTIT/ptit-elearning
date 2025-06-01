using System.Data.Common;
using Dapper;
using Elearning.Common.Application.Data;
using Elearning.Common.Application.Messaging;
using Elearning.Common.Domain;

namespace Elearning.Modules.Program.Application.Program.GetAssignmentSubmissions;

internal sealed class GetAssignmentSubmissionsQueryHandler(
    IDbConnectionFactory dbConnectionFactory) : IQueryHandler<GetAssignmentSubmissionsQuery, GetAssignmentSubmissionsResponse>
{
  public async Task<Result<GetAssignmentSubmissionsResponse>> Handle(GetAssignmentSubmissionsQuery request, CancellationToken cancellationToken)
  {
    try
    {
      await using DbConnection connection = await dbConnectionFactory.OpenConnectionAsync();

      // Verify teacher has access to this assignment
      var teacherAccessQuery = @"
                SELECT COUNT(*)
                FROM programs.table_assignments a
                INNER JOIN programs.table_teaching_assign_courses tac ON a.course_id = tac.id
                WHERE a.id = @AssignmentId AND tac.teacher_id = @TeacherId";

      var hasAccess = await connection.QuerySingleAsync<int>(teacherAccessQuery, new { request.AssignmentId, request.TeacherId });

      if (hasAccess == 0)
      {
        return Result.Failure<GetAssignmentSubmissionsResponse>(
            new Error("GetAssignmentSubmissions.Unauthorized", "You don't have access to this assignment", ErrorType.Authorization));
      }

      // Get assignment info
      var assignmentQuery = @"
                SELECT 
                    a.id,
                    a.title,
                    a.description,
                    a.deadline,
                    a.max_score,
                    COUNT(asub.id) as total_submissions,
                    COUNT(CASE WHEN asub.grade IS NOT NULL THEN 1 END) as graded_submissions,
                    COUNT(CASE WHEN asub.grade IS NULL THEN 1 END) as pending_submissions
                FROM programs.table_assignments a
                LEFT JOIN programs.table_assignment_submissions asub ON a.id = asub.assignment_id
                WHERE a.id = @AssignmentId
                GROUP BY a.id, a.title, a.description, a.deadline, a.max_score";

      var assignment = await connection.QuerySingleAsync<AssignmentInfoDto>(assignmentQuery, new { request.AssignmentId });

      // Build submissions query with filters
      var whereClause = "WHERE asub.assignment_id = @AssignmentId";
      var parameters = new Dictionary<string, object>
      {
        ["AssignmentId"] = request.AssignmentId,
        ["Offset"] = (request.Page - 1) * request.PageSize,
        ["PageSize"] = request.PageSize
      };

      if (!string.IsNullOrEmpty(request.Status))
      {
        whereClause += " AND asub.status = @Status";
        parameters["Status"] = request.Status;
      }

      // Get submissions with pagination
      var submissionsQuery = $@"
                SELECT 
                    asub.id,
                    asub.student_id,
                    u.full_name as student_name,
                    u.email as student_email,
                    u.avatar_url as student_avatar,
                    asub.submission_type,
                    asub.file_urls,
                    asub.text_content,
                    asub.submitted_at,
                    asub.grade,
                    asub.feedback,
                    asub.status,
                    CASE WHEN asub.submitted_at > a.deadline THEN true ELSE false END as is_late,
                    CASE WHEN asub.submitted_at > a.deadline 
                         THEN asub.submitted_at - a.deadline 
                         ELSE NULL END as late_duration
                FROM programs.table_assignment_submissions asub
                INNER JOIN users.table_users u ON asub.student_id = u.id
                INNER JOIN programs.table_assignments a ON asub.assignment_id = a.id
                {whereClause}
                ORDER BY 
                    CASE WHEN asub.grade IS NULL THEN 0 ELSE 1 END,
                    asub.submitted_at DESC
                LIMIT @PageSize OFFSET @Offset";

      var submissions = await connection.QueryAsync<AssignmentSubmissionDto>(submissionsQuery, parameters);

      // Get total count for pagination
      var countQuery = $@"
                SELECT COUNT(*)
                FROM programs.table_assignment_submissions asub
                {whereClause}";

      var totalCount = await connection.QuerySingleAsync<int>(countQuery, parameters);

      // Process file URLs (convert from JSON array to List<string>)
      foreach (var submission in submissions)
      {
        if (!string.IsNullOrEmpty(submission.FileUrls?.FirstOrDefault()))
        {
          try
          {
            submission.FileUrls = System.Text.Json.JsonSerializer.Deserialize<List<string>>(submission.FileUrls.FirstOrDefault() ?? "[]") ?? new List<string>();
          }
          catch
          {
            submission.FileUrls = new List<string>();
          }
        }
      }

      var response = new GetAssignmentSubmissionsResponse
      {
        Assignment = assignment,
        Submissions = submissions.ToList(),
        Pagination = new PaginationDto
        {
          Page = request.Page,
          PageSize = request.PageSize,
          TotalCount = totalCount,
          TotalPages = (int)Math.Ceiling((double)totalCount / request.PageSize),
          HasNextPage = request.Page * request.PageSize < totalCount,
          HasPreviousPage = request.Page > 1
        }
      };

      return Result.Success(response);
    }
    catch (Exception ex)
    {
      return Result.Failure<GetAssignmentSubmissionsResponse>(
          new Error("GetAssignmentSubmissions.DatabaseError", $"Database error: {ex.Message}", ErrorType.Failure));
    }
  }
}
