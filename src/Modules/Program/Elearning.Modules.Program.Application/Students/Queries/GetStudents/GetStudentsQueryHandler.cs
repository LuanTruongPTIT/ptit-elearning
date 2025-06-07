using System.Data.Common;
using Dapper;
using Elearning.Common.Application.Data;
using Elearning.Common.Application.Messaging;
using Elearning.Common.Domain;

namespace Elearning.Modules.Program.Application.Students.Queries.GetStudents;

internal sealed class GetStudentsQueryHandler(
    IDbConnectionFactory dbConnectionFactory
) : IQueryHandler<GetStudentsQuery, GetStudentsResponse>
{
    public async Task<Result<GetStudentsResponse>> Handle(
        GetStudentsQuery request,
        CancellationToken cancellationToken)
    {
        await using DbConnection connection = await dbConnectionFactory.OpenConnectionAsync();

        // Build WHERE conditions
        var whereConditions = new List<string> { "ur.role_name = 'Student'" };
        var parameters = new Dictionary<string, object>();

        if (!string.IsNullOrEmpty(request.SearchTerm))
        {
            whereConditions.Add("(u.full_name ILIKE @searchTerm OR u.email ILIKE @searchTerm)");
            parameters.Add("searchTerm", $"%{request.SearchTerm}%");
        }

        if (!string.IsNullOrEmpty(request.Status) && request.Status != "all")
        {
            whereConditions.Add("u.account_status = @status");
            parameters.Add("status", request.Status);
        }

        var whereClause = string.Join(" AND ", whereConditions);
        var offset = (request.Page - 1) * request.PageSize;

        // Count query
        var countSql = $"""
            SELECT COUNT(*)
            FROM users.table_users u
            JOIN users.table_user_roles ur ON u.id = ur.user_id
            LEFT JOIN programs.table_student_programs sp ON u.id = sp.student_id
            LEFT JOIN programs.table_programs p ON sp.program_id = p.id
            LEFT JOIN programs.table_departments d ON p.department_id = d.id
            WHERE {whereClause}
        """;

        // Data query
        var dataSql = $"""
            WITH student_stats AS (
                SELECT 
                    sp.student_id,
                    COUNT(DISTINCT tac.id) as courses_count,
                    COALESCE(AVG(
                        CASE 
                            WHEN scp.total_lectures > 0 
                            THEN (scp.completed_lectures::float / scp.total_lectures::float) * 10 
                            ELSE 0 
                        END
                    ), 0) as gpa
                FROM programs.table_student_programs sp
                LEFT JOIN programs.classes c ON sp.program_id = c.program_id
                LEFT JOIN programs.table_teaching_assign_courses tac ON c.id = tac.class_id
                LEFT JOIN programs.table_student_course_progress scp ON sp.student_id = scp.student_id 
                    AND tac.id = scp.teaching_assign_course_id
                GROUP BY sp.student_id
            )
            SELECT 
                u.id,
                u.full_name as name,
                u.email,
                u.phone_number AS PhoneNumber,
                u.date_of_birth,
                u.created_at as enrollment_date,
               CASE u.account_status
                   WHEN 1 THEN 'Active'
                   WHEN 0 THEN 'Inactive'
                   ELSE 'Unknown'
               END as status,
                COALESCE(ss.courses_count, 0) as CoursesCount,
                COALESCE(ss.gpa, 0) as gpa,
                COALESCE(d.name, 'Chưa xác định') as department
            FROM users.table_users u
            JOIN users.table_user_roles ur ON u.id = ur.user_id
            LEFT JOIN programs.table_student_programs sp ON u.id = sp.student_id
            LEFT JOIN programs.table_programs p ON sp.program_id = p.id
            LEFT JOIN programs.table_departments d ON p.department_id = d.id
            LEFT JOIN student_stats ss ON u.id = ss.student_id
            WHERE {whereClause}
            ORDER BY u.created_at DESC
            LIMIT @pageSize OFFSET @offset
        """;

        parameters.Add("pageSize", request.PageSize);
        parameters.Add("offset", offset);

        try
        {
            var totalCount = await connection.QuerySingleAsync<int>(countSql, parameters);
            var students = await connection.QueryAsync<StudentDto>(dataSql, parameters);

            var totalPages = (int)Math.Ceiling((double)totalCount / request.PageSize);

            var response = new GetStudentsResponse
            {
                Students = students.ToList(),
                TotalCount = totalCount,
                TotalPages = totalPages,
                CurrentPage = request.Page,
                HasNextPage = request.Page < totalPages,
                HasPreviousPage = request.Page > 1
            };

            return Result.Success(response);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            return Result.Failure<GetStudentsResponse>(
                Error.Failure("GetStudents.Error", ex.Message));
        }
    }
}
