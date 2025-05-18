using System.Data.Common;
using Dapper;
using Elearning.Common.Application.Data;
using Elearning.Common.Application.Messaging;
using Elearning.Common.Domain;

namespace Elearning.Modules.Program.Application.Program.GetStudentCourses;

internal sealed class GetStudentCoursesQueryHandler(
    IDbConnectionFactory dbConnectionFactory
) : IQueryHandler<GetStudentCoursesQuery, List<GetStudentCoursesResponse>>
{
    public async Task<Result<List<GetStudentCoursesResponse>>> Handle(
        GetStudentCoursesQuery request,
        CancellationToken cancellationToken)
    {
        await using DbConnection connection = await dbConnectionFactory.OpenConnectionAsync();

        const string sql = """
            WITH course_progress AS (
                SELECT
                    slp.lecture_id,
                    slp.is_completed,
                    l.teaching_assign_course_id
                FROM
                    enrollment.table_student_lecture_progresses slp
                JOIN
                    content_management.table_lectures l ON slp.lecture_id = l.id
                WHERE
                    slp.student_id = @student_id
            ),
            lecture_counts AS (
                SELECT
                    l.teaching_assign_course_id,
                    COUNT(l.id) AS total_lectures,
                    SUM(CASE WHEN cp.is_completed THEN 1 ELSE 0 END) AS completed_lectures
                FROM
                    content_management.table_lectures l
                LEFT JOIN
                    course_progress cp ON l.id = cp.lecture_id
                GROUP BY
                    l.teaching_assign_course_id
            )
            SELECT
                tac.id AS course_id,
                tac.course_class_name AS course_name,
                c.code AS course_code,
                tac.thumbnail_url,
                tac.description,
                u.full_name AS teacher_name,
                CASE
                    WHEN lc.total_lectures = 0 THEN 0
                    ELSE ROUND((COALESCE(lc.completed_lectures, 0)::float / lc.total_lectures::float) * 100)
                END AS progress_percentage,
                se.enrollment_date
            FROM
                enrollment.table_student_enrollments se
            JOIN
                programs.table_teaching_assign_courses tac ON se.teaching_assign_course_id = tac.id
            JOIN
                programs.table_courses c ON tac.course_id = c.id
            JOIN
                users.table_users u ON tac.teacher_id = u.id
            LEFT JOIN
                lecture_counts lc ON tac.id = lc.teaching_assign_course_id
            WHERE
                se.student_id = @student_id
                AND se.status = 'active'
            ORDER BY
                se.enrollment_date DESC
        """;

        try
        {
            var result = await connection.QueryAsync<GetStudentCoursesResponse>(
                sql,
                new { request.student_id }
            );

            return Result.Success(result.ToList());
        }
        catch (Exception ex)
        {
            return Result.Failure<List<GetStudentCoursesResponse>>(
                Error.Failure("GetStudentCourses.Error", ex.Message));
        }
    }
}
