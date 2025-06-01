using System.Data.Common;
using System.Text.Json;
using Dapper;
using Elearning.Common.Application.Data;
using Elearning.Common.Application.Messaging;
using Elearning.Common.Domain;

namespace Elearning.Modules.Program.Application.Program.GetAssignmentsByCourse;

internal sealed class GetAssignmentsByCourseQueryHandler(
    IDbConnectionFactory dbConnectionFactory) : IQueryHandler<GetAssignmentsByCourseQuery, List<GetAssignmentsByCourseResponse>>
{
  public async Task<Result<List<GetAssignmentsByCourseResponse>>> Handle(GetAssignmentsByCourseQuery request, CancellationToken cancellationToken)
  {
    try
    {
      await using DbConnection connection = await dbConnectionFactory.OpenConnectionAsync();

      // Verify teacher has access to this course
      var teacherAccessQuery = @"
                SELECT COUNT(*)
                FROM programs.table_teaching_assign_courses tac
                WHERE tac.id = @CourseId AND tac.teacher_id = @TeacherId";

      var hasAccess = await connection.QuerySingleAsync<int>(teacherAccessQuery, new { request.CourseId, request.TeacherId });

      if (hasAccess == 0)
      {
        return Result.Failure<List<GetAssignmentsByCourseResponse>>(
            new Error("GetAssignmentsByCourse.Unauthorized", "You don't have access to this course", ErrorType.Authorization));
      }

      // Get assignments for the course
      var assignmentsQuery = @"
                SELECT 
                    a.id,
                    a.title,
                    a.description,
                    a.deadline,
                    a.assignment_type,
                    a.show_answers,
                    a.time_limit_minutes,
                    a.attachment_urls,
                    a.max_score,
                    a.is_published,
                    a.created_at,
                    a.updated_at,
                    a.created_by,
                    COUNT(DISTINCT asub.id) as submissions_count,
                    COUNT(DISTINCT sp.student_id) as total_students
                FROM programs.table_assignments a
                INNER JOIN programs.table_teaching_assign_courses tac ON a.teaching_assign_course_id = tac.id
                INNER JOIN programs.classes c ON c.id = tac.class_id
                LEFT JOIN programs.table_student_programs sp ON sp.program_id = c.program_id
                LEFT JOIN programs.table_assignment_submissions asub ON a.id = asub.assignment_id
                WHERE tac.id = @CourseId
                GROUP BY a.id, a.title, a.description, a.deadline, a.assignment_type, 
                         a.show_answers, a.time_limit_minutes, a.attachment_urls, 
                         a.max_score, a.is_published, a.created_at, a.updated_at, a.created_by
                ORDER BY a.created_at DESC";

      var assignments = await connection.QueryAsync<dynamic>(assignmentsQuery, new { request.CourseId });

      var result = assignments.Select(a => new GetAssignmentsByCourseResponse(
          (Guid)a.id,
          (string)a.title,
          (string)(a.description ?? ""),
          (DateTime)a.deadline,
          (string)a.assignment_type,
          (bool)a.show_answers,
          (int?)a.time_limit_minutes,
          ParseAttachmentUrls(a.attachment_urls),
          (decimal)a.max_score,
          (bool)a.is_published,
          (DateTime)a.created_at,
          (DateTime)a.updated_at,
          (Guid)a.created_by,
          (int)a.submissions_count,
          (int)a.total_students
      )).ToList();

      return Result.Success(result);
    }
    catch (Exception ex)
    {

      Console.WriteLine(ex);
      return Result.Failure<List<GetAssignmentsByCourseResponse>>(
                      new Error("GetAssignmentsByCourse.DatabaseError", $"Database error: {ex.Message}", ErrorType.Failure));
    }
  }

  private static List<string>? ParseAttachmentUrls(object? attachmentUrlsData)
  {
    if (attachmentUrlsData == null || attachmentUrlsData == DBNull.Value)
      return null;

    try
    {
      // Handle different data types from database
      switch (attachmentUrlsData)
      {
        case string[] stringArray:
          return stringArray.ToList();

        case string jsonString when !string.IsNullOrEmpty(jsonString):
          // Try to parse as JSON array
          if (jsonString.StartsWith("[") && jsonString.EndsWith("]"))
          {
            return JsonSerializer.Deserialize<List<string>>(jsonString);
          }
          // If it's a single string, return as single item list
          return new List<string> { jsonString };

        case string emptyString when string.IsNullOrEmpty(emptyString):
          return null;

        default:
          // Try to convert to string and then parse
          var stringValue = attachmentUrlsData.ToString();
          if (string.IsNullOrEmpty(stringValue))
            return null;

          if (stringValue.StartsWith("[") && stringValue.EndsWith("]"))
          {
            return JsonSerializer.Deserialize<List<string>>(stringValue);
          }

          return new List<string> { stringValue };
      }
    }
    catch (Exception ex)
    {
      // Log the error for debugging
      Console.WriteLine($"Error parsing attachment URLs: {ex.Message}, Data: {attachmentUrlsData}");
      return null;
    }
  }
}
