using System.Data.Common;
using Dapper;
using Elearning.Common.Application.Data;
using Elearning.Common.Application.Messaging;
using Elearning.Common.Domain;

namespace Elearning.Modules.Users.Application.Students.GetStudents;

internal sealed class GetStudentsQueryHandler(
    IDbConnectionFactory dbConnectionFactory) : IQueryHandler<GetStudentsQuery, GetStudentsWithPaginationResponse>
{
  public async Task<Result<GetStudentsWithPaginationResponse>> Handle(GetStudentsQuery request, CancellationToken cancellationToken)
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
        return Result.Failure<GetStudentsWithPaginationResponse>(
            new Error("GetStudents.Unauthorized", "You don't have permission to view students", ErrorType.Authorization));
      }

      Console.WriteLine($"GetStudents: UserRole={userRole}, UserId={userId}");

      // Default values for pagination
      int page = request.Page ?? 1;
      int pageSize = Math.Min(request.PageSize ?? 10, 100); // Limit max page size to 100
      int offset = (page - 1) * pageSize;

      // Build base SQL query
      string baseQuery = @"
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
                        ur.role_name = 'Student'";

      string countQuery = @"
                    SELECT COUNT(*)
                    FROM 
                        users.table_users u
                    JOIN 
                        users.table_user_roles ur ON u.id = ur.user_id
                    LEFT JOIN 
                        programs.table_student_programs sp ON u.id = sp.student_id
                    LEFT JOIN 
                        programs.table_programs p ON sp.program_id = p.id
                    WHERE 
                        ur.role_name = 'Student'";

      // Add search filter if provided
      if (!string.IsNullOrEmpty(request.Keyword))
      {
        string searchFilter = @"
                        AND (u.full_name ILIKE '%' || @Keyword || '%' OR 
                            u.email ILIKE '%' || @Keyword || '%' OR
                            u.username ILIKE '%' || @Keyword || '%' OR
                            u.phone_number ILIKE '%' || @Keyword || '%' OR
                            p.name ILIKE '%' || @Keyword || '%')";

        baseQuery += searchFilter;
        countQuery += searchFilter;
      }

      // Add program filter if provided
      if (request.ProgramId.HasValue)
      {
        string programFilter = " AND sp.program_id = @ProgramId";
        baseQuery += programFilter;
        countQuery += programFilter;
      }

      // Add status filter if provided
      if (request.AccountStatus.HasValue)
      {
        string statusFilter = " AND u.account_status = @AccountStatus";
        baseQuery += statusFilter;
        countQuery += statusFilter;
      }

      // Role-based access control
      if (userRole == "Teacher" || userRole == "Lecturer")
      {
        // Teachers can only see students in their classes
        string teacherFilter = @"
                        AND EXISTS (
                            SELECT 1 FROM programs.table_teaching_assign_courses tac
                            INNER JOIN programs.classes c ON c.id = tac.class_id
                            INNER JOIN programs.table_student_programs sp2 ON sp2.program_id = c.program_id
                            WHERE tac.teacher_id = @UserId AND sp2.student_id = u.id
                        )";

        baseQuery += teacherFilter;
        countQuery += teacherFilter;
      }
      else if (userRole != "Administrator")
      {
        // Only Admin and Teachers can access this endpoint
        return Result.Failure<GetStudentsWithPaginationResponse>(
            new Error("GetStudents.Unauthorized", "You don't have permission to view students", ErrorType.Authorization));
      }

      // Add sorting and pagination
      baseQuery += @"
                    ORDER BY 
                        CASE WHEN @SortBy = 'full_name' AND @SortOrder = 'asc' THEN u.full_name END ASC,
                        CASE WHEN @SortBy = 'full_name' AND @SortOrder = 'desc' THEN u.full_name END DESC,
                        CASE WHEN @SortBy = 'email' AND @SortOrder = 'asc' THEN u.email END ASC,
                        CASE WHEN @SortBy = 'email' AND @SortOrder = 'desc' THEN u.email END DESC,
                        CASE WHEN @SortBy = 'created_at' AND @SortOrder = 'asc' THEN u.created_at END ASC,
                        CASE WHEN @SortBy = 'created_at' AND @SortOrder = 'desc' THEN u.created_at END DESC,
                        CASE WHEN @SortBy = 'program_name' AND @SortOrder = 'asc' THEN p.name END ASC,
                        CASE WHEN @SortBy = 'program_name' AND @SortOrder = 'desc' THEN p.name END DESC,
                        u.created_at DESC
                    LIMIT @PageSize OFFSET @Offset";

      var parameters = new
      {
        Keyword = request.Keyword,
        ProgramId = request.ProgramId,
        AccountStatus = request.AccountStatus,
        UserId = userId,
        SortBy = request.SortBy ?? "created_at",
        SortOrder = request.SortOrder ?? "desc",
        PageSize = pageSize,
        Offset = offset
      };

      // Execute queries sequentially (PostgreSQL doesn't support multiple concurrent commands on same connection)
      var totalCount = await connection.QuerySingleAsync<int>(countQuery, parameters);
      var students = await connection.QueryAsync<GetStudentsResponse>(baseQuery, parameters);

      var response = new GetStudentsWithPaginationResponse
      {
        Students = students.ToList(),
        TotalCount = totalCount,
        Page = page,
        PageSize = pageSize,
        TotalPages = (int)Math.Ceiling((double)totalCount / pageSize),
        HasNextPage = page * pageSize < totalCount,
        HasPreviousPage = page > 1
      };

      Console.WriteLine($"GetStudents: Found {students.Count()} students, Total: {totalCount}");

      return Result.Success(response);
    }
    catch (Exception ex)
    {
      Console.WriteLine($"Exception in GetStudentsQueryHandler: {ex}");

      return Result.Failure<GetStudentsWithPaginationResponse>(
          new Error("GetStudents.DatabaseError", $"Database error: {ex.Message}", ErrorType.Failure));
    }
  }
}
