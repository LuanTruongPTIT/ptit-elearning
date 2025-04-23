using System.Data.Common;
using Dapper;
using Elearning.Common.Application.Data;
using Elearning.Common.Application.Messaging;
using Elearning.Common.Domain;

namespace Elearning.Modules.Program.Application.Subjects.GetSubjects;

internal sealed class GetSubjectsQueryHandler(
    IDbConnectionFactory dbConnectionFactory) : IQueryHandler<GetSubjectsQuery, List<GetSubjectsResponse>>
{
  public async Task<Result<List<GetSubjectsResponse>>> Handle(GetSubjectsQuery request, CancellationToken cancellationToken)
  {
    try
    {
      await using DbConnection connection = await dbConnectionFactory.OpenConnectionAsync();

      // Validate user role and ID
      var userRole = request.UserRole;
      var userId = request.UserId;

      if (string.IsNullOrEmpty(userRole) || string.IsNullOrEmpty(userId.ToString()))
      {
        return Result.Failure<List<GetSubjectsResponse>>(
            new Error("GetSubjects.Unauthorized", "You don't have permission to view subjects", ErrorType.Authorization));
      }

      // Default values for pagination
      int page = request.Page ?? 1;
      int pageSize = request.PageSize ?? 10;
      int offset = (page - 1) * pageSize;

      // Build SQL query based on user role
      string sql;
      object parameters;

      if (userRole == "Administrator")
      {
        // Admin can see all subjects
        sql = @"
                    SELECT DISTINCT
                        c.id,
                        c.code,
                        c.name,
                        d.id as department_id,
                        d.name as department_name,
                        c.description,
                        c.is_active,
                        c.created_at,
                        c.updated_at
                    FROM 
                        programs.table_courses c
                    LEFT JOIN 
                        programs.table_program_courses pc ON c.id = pc.course_id
                    LEFT JOIN 
                        programs.table_programs p ON pc.program_id = p.id
                    LEFT JOIN 
                        programs.table_departments d ON p.department_id = d.id
                    WHERE 
                        (@Keyword IS NULL OR 
                        c.name ILIKE '%' || @Keyword || '%' OR 
                        c.code ILIKE '%' || @Keyword || '%' OR
                        d.name ILIKE '%' || @Keyword || '%')
                    ORDER BY 
                        c.created_at DESC
                    LIMIT @PageSize OFFSET @Offset
                ";

        parameters = new
        {
          Keyword = request.Keyword,
          PageSize = pageSize,
          Offset = offset
        };
      }
      else if (userRole == "Lecturer" || userRole == "Teacher")
      {
        // Teacher can only see subjects they teach
        sql = @"
                    SELECT DISTINCT
                        c.id,
                        c.code,
                        c.name,
                        d.id as department_id,
                        d.name as department_name,
                        c.description,
                        c.is_active,
                        c.created_at,
                        c.updated_at
                    FROM 
                        programs.table_courses c
                    JOIN 
                        programs.table_teaching_assign_courses tac ON c.id = tac.course_id
                    LEFT JOIN 
                        programs.table_program_courses pc ON c.id = pc.course_id
                    LEFT JOIN 
                        programs.table_programs p ON pc.program_id = p.id
                    LEFT JOIN 
                        programs.table_departments d ON p.department_id = d.id
                    WHERE 
                        tac.teacher_id = @TeacherId
                        AND (@Keyword IS NULL OR 
                        c.name ILIKE '%' || @Keyword || '%' OR 
                        c.code ILIKE '%' || @Keyword || '%' OR
                        d.name ILIKE '%' || @Keyword || '%')
                    ORDER BY 
                        c.created_at DESC
                    LIMIT @PageSize OFFSET @Offset
                ";

        parameters = new
        {
          TeacherId = userId,
          Keyword = request.Keyword,
          PageSize = pageSize,
          Offset = offset
        };
      }
      else
      {
        // Other roles can't see subjects
        return Result.Failure<List<GetSubjectsResponse>>(
            new Error("GetSubjects.Unauthorized", "You don't have permission to view subjects", ErrorType.Authorization));
      }

      // Execute query and return results
      var subjects = await connection.QueryAsync<GetSubjectsResponse>(sql, parameters);
      return Result.Success(subjects.ToList());
    }
    catch (Exception ex)
    {
      // Log the exception
      Console.WriteLine($"Error in GetSubjectsQueryHandler: {ex.Message}");
      Console.WriteLine($"Stack trace: {ex.StackTrace}");

      // Return failure result
      return Result.Failure<List<GetSubjectsResponse>>(
          new Error("GetSubjects.Error", $"An error occurred while retrieving subjects: {ex.Message}", ErrorType.Problem));
    }
  }
}
