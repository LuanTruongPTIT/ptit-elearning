using System.Data.Common;
using Dapper;
using Elearning.Common.Application.Data;
using Elearning.Common.Application.Messaging;
using Elearning.Common.Domain;

namespace Elearning.Modules.Users.Application.Students.GetStudents;

internal sealed class GetStudentsQueryHandler(
    IDbConnectionFactory dbConnectionFactory) : IQueryHandler<GetStudentsQuery, List<GetStudentsResponse>>
{
  public async Task<Result<List<GetStudentsResponse>>> Handle(GetStudentsQuery request, CancellationToken cancellationToken)
  {
    try
    {
      await using DbConnection connection = await dbConnectionFactory.OpenConnectionAsync();

      // Validate user role and ID
      var userRole = request.UserRole;
      var userId = request.UserId;

      if (string.IsNullOrEmpty(userRole) || string.IsNullOrEmpty(userId?.ToString()))
      {
        Console.WriteLine("Invalid user role or ID");
        return Result.Failure<List<GetStudentsResponse>>(
            new Error("GetStudents.Unauthorized", "You don't have permission to view students", ErrorType.Authorization));
      }

      Console.WriteLine("Connection opened");
      // Default values for pagination
      int page = request.Page ?? 1;
      int pageSize = request.PageSize ?? 10;
      int offset = (page - 1) * pageSize;

      // Build SQL query based on user role
      string sql;
      object parameters;

      if (userRole == "Administrator")
      {
        // Admin can see all students
        sql = @"
                    SELECT 
                        u.id,
                        u.username,
                        u.email,
                        u.full_name,
                        u.phone_number,
                        u.address,
                        u.avatar_url,
                        u.date_of_birth,
                        u.gender,
                        u.account_status,
                        u.created_at,
                        sp.program_id,
                        p.name as program_name
                    FROM 
                        users.table_users u
                    JOIN 
                        users.table_user_roles ur ON u.id = ur.user_id
                    LEFT JOIN 
                        programs.table_student_programs sp ON u.id = sp.student_id
                    LEFT JOIN 
                        programs.table_programs p ON sp.program_id = p.id
                    WHERE 
                        ur.role_name = 'Student'
                        AND (@Keyword IS NULL OR 
                            u.full_name ILIKE '%' || @Keyword || '%' OR 
                            u.email ILIKE '%' || @Keyword || '%' OR
                            u.username ILIKE '%' || @Keyword || '%' OR
                            u.phone_number ILIKE '%' || @Keyword || '%')
                    ORDER BY 
                        u.created_at DESC
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
        // For now, teachers see all students too (simplified)
        // In a real implementation, you would filter by students they teach
        sql = @"
                    SELECT 
                        u.id,
                        u.username,
                        u.email,
                        u.full_name,
                        u.phone_number,
                        u.address,
                        u.avatar_url,
                        u.date_of_birth,
                        u.gender,
                        u.account_status,
                        u.created_at,
                        sp.program_id,
                        p.name as program_name
                    FROM 
                        users.table_users u
                    JOIN 
                        users.table_user_roles ur ON u.id = ur.user_id
                    LEFT JOIN 
                        programs.table_student_programs sp ON u.id = sp.student_id
                    LEFT JOIN 
                        programs.table_programs p ON sp.program_id = p.id
                    WHERE 
                        ur.role_name = 'Student'
                        AND (@Keyword IS NULL OR 
                            u.full_name ILIKE '%' || @Keyword || '%' OR 
                            u.email ILIKE '%' || @Keyword || '%' OR
                            u.username ILIKE '%' || @Keyword || '%' OR
                            u.phone_number ILIKE '%' || @Keyword || '%')
                    ORDER BY 
                        u.created_at DESC
                    LIMIT @PageSize OFFSET @Offset
                ";

        parameters = new
        {
          Keyword = request.Keyword,
          PageSize = pageSize,
          Offset = offset
        };
      }
      else
      {
        // Other roles can't see students
        return Result.Failure<List<GetStudentsResponse>>(
            new Error("GetStudents.Unauthorized", "You don't have permission to view students", ErrorType.Authorization));
      }

      // Execute query and return results
      var students = await connection.QueryAsync<GetStudentsResponse>(sql, parameters);
      return Result.Success(students.ToList());
    }
    catch (Exception ex)
    {
      Console.WriteLine("Error in GetStudentsQueryHandler");
      // Log the exception
      Console.WriteLine($"Error in GetStudentsQueryHandler: {ex.Message}");
      Console.WriteLine($"Stack trace: {ex.StackTrace}");

      // Return failure result
      return Result.Failure<List<GetStudentsResponse>>(
          new Error("GetStudents.Error", $"An error occurred while retrieving students: {ex.Message}", ErrorType.Failure));
    }
  }
}
