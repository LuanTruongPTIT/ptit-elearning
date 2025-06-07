using System.Data.Common;
using Dapper;
using Elearning.Common.Application.Data;
using Elearning.Common.Application.Messaging;
using Elearning.Common.Domain;

namespace Elearning.Modules.Program.Application.Courses.Queries.GetAdminCourses;

internal sealed class GetAdminCoursesQueryHandler(
    IDbConnectionFactory dbConnectionFactory
) : IQueryHandler<GetAdminCoursesQuery, GetAdminCoursesResponse>
{
    public async Task<Result<GetAdminCoursesResponse>> Handle(
    GetAdminCoursesQuery request,
    CancellationToken cancellationToken)
    {
        await using DbConnection connection = await dbConnectionFactory.OpenConnectionAsync();

        try
        {
            // Base query for FROM and JOINs
            var baseQuery = """
            FROM programs.table_teaching_assign_courses tac
            JOIN programs.table_courses c ON tac.course_id = c.id
            JOIN programs.classes cl ON tac.class_id = cl.id
            JOIN programs.table_programs p ON cl.program_id = p.id
            LEFT JOIN programs.table_departments d ON p.department_id = d.id
            LEFT JOIN users.table_users u ON tac.teacher_id = u.id
            LEFT JOIN (
                SELECT 
                    sp.program_id,
                    COUNT(DISTINCT sp.student_id) AS students_count
                FROM programs.table_student_programs sp
                GROUP BY sp.program_id
            ) student_counts ON cl.program_id = student_counts.program_id
            LEFT JOIN (
                SELECT 
                    scp.teaching_assign_course_id,
                    AVG(scp.progress_percentage) AS completion_rate
                FROM programs.table_student_course_progress scp
                GROUP BY scp.teaching_assign_course_id
            ) completion_stats ON tac.id = completion_stats.teaching_assign_course_id
            WHERE 1=1
        """;

            var whereConditions = new List<string>();
            var parameters = new DynamicParameters();

            // Search filters
            if (!string.IsNullOrWhiteSpace(request.SearchTerm))
            {
                whereConditions.Add("""
                (LOWER(c.name) LIKE @searchTerm 
                 OR LOWER(c.code) LIKE @searchTerm 
                 OR LOWER(u.full_name) LIKE @searchTerm
                 OR LOWER(cl.class_name) LIKE @searchTerm)
            """);
                parameters.Add("searchTerm", $"%{request.SearchTerm.ToLower()}%");
            }

            // Status filter
            if (!string.IsNullOrWhiteSpace(request.Status))
            {
                whereConditions.Add("tac.status = @status");
                parameters.Add("status", request.Status);
            }

            // Department filter
            if (!string.IsNullOrWhiteSpace(request.Department))
            {
                whereConditions.Add("d.name = @department");
                parameters.Add("department", request.Department);
            }

            // Combine WHERE clause
            var whereClause = whereConditions.Count > 0
                ? " AND " + string.Join(" AND ", whereConditions)
                : "";

            // Count query with DISTINCT to avoid duplication due to joins
            var countSql = $"""
            SELECT COUNT(DISTINCT tac.id)
            {baseQuery}
            {whereClause}
        """;

            var totalCount = await connection.QuerySingleAsync<int>(countSql, parameters);

            var totalPages = (int)Math.Ceiling(totalCount / (double)request.PageSize);
            var offset = (request.Page - 1) * request.PageSize;

            // Data query
            var dataSql = $"""
            SELECT 
                tac.id,
                CONCAT(c.name, ' - ', cl.class_name) AS name,
                tac.description,
                c.code,
                COALESCE(u.full_name, 'Chưa phân công') AS instructor,
                COALESCE(d.name, 'Chưa xác định') AS department,
                tac.created_at AS start_date,
                tac.end_date,
                tac.status,
                50 AS max_students,
               120 AS duration,
                ROUND(CAST(4.0 + (RANDOM() * 1.0) AS numeric), 1) AS rating,
                COALESCE(tac.thumbnail_url, '') AS thumbnail_url,
                COALESCE(student_counts.students_count, 0) AS students_count,
                COALESCE(completion_stats.completion_rate, 0) AS completion_rate
            {baseQuery}
            {whereClause}
            ORDER BY tac.created_at DESC
            LIMIT @pageSize OFFSET @offset
        """;

            parameters.Add("pageSize", request.PageSize);
            parameters.Add("offset", offset);

            var courses = await connection.QueryAsync(dataSql, parameters);

            var coursesList = courses.Select(course => new AdminCourseDto
            {
                Id = Guid.Parse(course.id.ToString()),
                Name = course.name ?? "Unknown Course",
                Description = course.description ?? "",
                Code = course.code ?? "",
                Instructor = course.instructor ?? "Unassigned",
                Department = course.department ?? "Unknown",
                StartDate = course.start_date ?? DateTime.MinValue,
                EndDate = course.end_date,
                Status = course.status ?? "Active",
                StudentsCount = (int)(course.students_count ?? 0),
                MaxStudents = (int)(course.max_students ?? 0),
                Duration = (int)(course.duration ?? 0),
                Rating = (decimal)(course.rating ?? 0),
                CompletionRate = (decimal)(course.completion_rate ?? 0),
                ThumbnailUrl = course.thumbnail_url ?? ""
            }).ToList();
            var response = new GetAdminCoursesResponse
            {
                Courses = coursesList,
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
            Console.WriteLine($"Error in GetAdminCoursesQueryHandler: {ex}");
            return Result.Failure<GetAdminCoursesResponse>(
                Error.Failure("GetAdminCourses.Error", ex.Message));
        }
    }

}
