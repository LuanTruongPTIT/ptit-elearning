using System.Data.Common;
using Dapper;
using Elearning.Common.Application.Data;
using Elearning.Common.Application.Messaging;
using Elearning.Common.Domain;

namespace Elearning.Modules.Users.Application.Students.GetStudent;

internal sealed class GetStudentQueryHandler(
    IDbConnectionFactory dbConnectionFactory) : IQueryHandler<GetStudentQuery, GetStudentResponse>
{
  public async Task<Result<GetStudentResponse>> Handle(GetStudentQuery request, CancellationToken cancellationToken)
  {
    try
    {
      await using DbConnection connection = await dbConnectionFactory.OpenConnectionAsync();

      var studentId = request.StudentId;
      var userRole = request.UserRole;
      var userId = request.UserId;

      // Check if user role and ID are provided
      if (string.IsNullOrEmpty(userRole) || string.IsNullOrEmpty(userId))
      {
        return Result.Failure<GetStudentResponse>(
            new Error("GetStudent.Unauthorized", "You don't have permission to view this student", ErrorType.Authorization));
      }

      string sql;
      object parameters;

      if (userRole == "Administrator")
      {
        // Admin can see any student
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
                        AND u.id = @StudentId
                ";

        parameters = new
        {
          StudentId = Guid.Parse(studentId)
        };
      }
      else if (userRole == "Lecturer" || userRole == "Teacher")
      {
        // For now, teachers can see any student (simplified)
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
                        AND u.id = @StudentId
                ";

        parameters = new
        {
          StudentId = Guid.Parse(studentId)
        };
      }
      else
      {
        // Other roles can't see students
        return Result.Failure<GetStudentResponse>(
            new Error("GetStudent.Unauthorized", "You don't have permission to view this student", ErrorType.Authorization));
      }

      var student = await connection.QueryFirstOrDefaultAsync<GetStudentResponse>(sql, parameters);

      if (student == null)
      {
        return Result.Failure<GetStudentResponse>(
            new Error("GetStudent.NotFound", $"Student with ID {request.StudentId} not found", ErrorType.NotFound));
      }

      return Result.Success(student);
    }
    catch (Exception ex)
    {
      // Log the exception
      Console.WriteLine($"Error in GetStudentQueryHandler: {ex.Message}");
      Console.WriteLine($"Stack trace: {ex.StackTrace}");

      // Return failure result
      return Result.Failure<GetStudentResponse>(
          new Error("GetStudent.Error", $"An error occurred while retrieving student: {ex.Message}", ErrorType.Failure));
    }
  }
}
