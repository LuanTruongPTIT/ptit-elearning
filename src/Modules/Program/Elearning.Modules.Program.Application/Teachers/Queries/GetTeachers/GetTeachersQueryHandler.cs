using System.Data.Common;
using Dapper;
using Elearning.Common.Application.Data;
using Elearning.Common.Application.Messaging;
using Elearning.Common.Domain;

namespace Elearning.Modules.Program.Application.Teachers.Queries.GetTeachers;

internal sealed class GetTeachersQueryHandler(
    IDbConnectionFactory dbConnectionFactory
) : IQueryHandler<GetTeachersQuery, GetTeachersResponse>
{
    public async Task<Result<GetTeachersResponse>> Handle(
        GetTeachersQuery request,
        CancellationToken cancellationToken)
    {
        await using DbConnection connection = await dbConnectionFactory.OpenConnectionAsync();

        // Build WHERE conditions
        var whereConditions = new List<string> { "ur.role_name IN ('Teacher', 'Lecturer')" };
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

        if (!string.IsNullOrEmpty(request.Department) && request.Department != "all")
        {
            whereConditions.Add("d.name = @department");
            parameters.Add("department", request.Department);
        }

        var whereClause = string.Join(" AND ", whereConditions);
        var offset = (request.Page - 1) * request.PageSize;

        // Count query
        var countSql = $"""
            SELECT COUNT(DISTINCT u.id)
            FROM users.table_users u
            JOIN users.table_user_roles ur ON u.id = ur.user_id
            LEFT JOIN programs.table_teaching_assign_courses tac ON u.id = tac.teacher_id
            LEFT JOIN programs.classes c ON tac.class_id = c.id
            LEFT JOIN programs.table_programs p ON c.program_id = p.id
            LEFT JOIN programs.table_departments d ON p.department_id = d.id
            WHERE {whereClause}
        """;

        // Data query
        var dataSql = $"""
            WITH teacher_stats AS (
                SELECT 
                    u.id,
                    COUNT(DISTINCT tac.id) as courses_count,
                    COUNT(DISTINCT sp.student_id) as students_count,
                    -- Mock rating calculation (you can replace with actual rating logic)
                    ROUND((4.0 + RANDOM() * 1.0)::numeric, 1) as rating
                FROM users.table_users u
                JOIN users.table_user_roles ur ON u.id = ur.user_id
                LEFT JOIN programs.table_teaching_assign_courses tac ON u.id = tac.teacher_id
                LEFT JOIN programs.classes c ON tac.class_id = c.id
                LEFT JOIN programs.table_student_programs sp ON c.program_id = sp.program_id
                WHERE ur.role_name IN ('Teacher', 'Lecturer')
                GROUP BY u.id
            )
            SELECT 
                u.id,
                u.full_name as name,
                u.email,
                u.phone_number,
                u.created_at as join_date,
                u.account_status as status,
                COALESCE(d.name, 'Chưa xác định') as department,
                COALESCE(ts.courses_count, 0) as courses_count,
                COALESCE(ts.students_count, 0) as students_count,
                COALESCE(ts.rating, 4.0) as rating
            FROM users.table_users u
            JOIN users.table_user_roles ur ON u.id = ur.user_id
            LEFT JOIN programs.table_teaching_assign_courses tac ON u.id = tac.teacher_id
            LEFT JOIN programs.classes c ON tac.class_id = c.id
            LEFT JOIN programs.table_programs p ON c.program_id = p.id
            LEFT JOIN programs.table_departments d ON p.department_id = d.id
            LEFT JOIN teacher_stats ts ON u.id = ts.id
            WHERE {whereClause}
            GROUP BY u.id, u.full_name, u.email, u.phone_number, u.created_at, u.account_status, d.name, ts.courses_count, ts.students_count, ts.rating
            ORDER BY u.created_at DESC
            LIMIT @pageSize OFFSET @offset
        """;

        parameters.Add("pageSize", request.PageSize);
        parameters.Add("offset", offset);

        try
        {
            var totalCount = await connection.QuerySingleAsync<int>(countSql, parameters);
            var teachers = await connection.QueryAsync<TeacherDto>(dataSql, parameters);

            var totalPages = (int)Math.Ceiling((double)totalCount / request.PageSize);

            var response = new GetTeachersResponse
            {
                Teachers = teachers.ToList(),
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
            return Result.Failure<GetTeachersResponse>(
                      Error.Failure("GetTeachers.Error", ex.Message));
        }
    }
}
