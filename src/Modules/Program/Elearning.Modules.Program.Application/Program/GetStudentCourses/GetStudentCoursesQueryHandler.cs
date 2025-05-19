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
            -- First, find the student's program
            WITH student_program AS (
                SELECT
                    sp.student_id,
                    sp.program_id
                FROM
                    programs.table_student_programs sp
                WHERE
                    sp.student_id = @student_id
            ),
            -- Find classes associated with the student's program
            student_classes AS (
                SELECT
                    c.id AS class_id,
                    c.program_id,
                    sp.student_id
                FROM
                    programs.classes c
                JOIN
                    student_program sp ON c.program_id = sp.program_id
                WHERE
                    c.status = 'active'
            ),
            -- Find teaching assign courses for those classes
            student_courses AS (
                SELECT
                    tac.id AS teaching_assign_course_id,
                    tac.course_id,
                    tac.teacher_id,
                    tac.course_class_name,
                    tac.description,
                    tac.thumbnail_url,
                    tac.start_date,
                    sc.student_id,
                    CURRENT_TIMESTAMP AS enrollment_date
                FROM
                    programs.table_teaching_assign_courses tac
                JOIN
                    student_classes sc ON tac.class_id = sc.class_id
                WHERE
                    tac.status = 'active'
            ),
            -- Calculate lecture progress from student_lecture_progresses (if table exists)
            course_progress AS (
                SELECT
                    l.id AS lecture_id,
                    FALSE AS is_completed, -- Mặc định là chưa hoàn thành
                    l.teaching_assign_course_id
                FROM
                    programs.table_lectures l
                WHERE
                    l.teaching_assign_course_id IN (SELECT teaching_assign_course_id FROM student_courses)
            ),
            lecture_counts AS (
                SELECT
                    l.teaching_assign_course_id,
                    COUNT(l.id) AS total_lectures,
                    0 AS completed_lectures -- Mặc định là 0 bài giảng đã hoàn thành
                FROM
                    programs.table_lectures l
                WHERE
                    l.teaching_assign_course_id IN (SELECT teaching_assign_course_id FROM student_courses)
                GROUP BY
                    l.teaching_assign_course_id
            ),
            -- Calculate progress and update student_course_progress table
            progress_data AS (
                INSERT INTO programs.table_student_course_progress (
                    student_id,
                    teaching_assign_course_id,
                    total_lectures,
                    completed_lectures,
                    progress_percentage,
                    status,
                    last_accessed
                )
                SELECT
                    sc.student_id,
                    sc.teaching_assign_course_id,
                    COALESCE(lc.total_lectures, 0),
                    COALESCE(lc.completed_lectures, 0),
                    CASE
                        WHEN COALESCE(lc.total_lectures, 0) = 0 THEN 0
                        ELSE ROUND((COALESCE(lc.completed_lectures, 0)::float / lc.total_lectures::float) * 100)
                    END,
                    CASE
                        WHEN COALESCE(lc.total_lectures, 0) = 0 THEN 'not_started'
                        WHEN COALESCE(lc.completed_lectures, 0) = lc.total_lectures THEN 'completed'
                        ELSE 'in_progress'
                    END,
                    CURRENT_TIMESTAMP
                FROM
                    student_courses sc
                LEFT JOIN
                    lecture_counts lc ON sc.teaching_assign_course_id = lc.teaching_assign_course_id
                ON CONFLICT (student_id, teaching_assign_course_id) DO UPDATE SET
                    total_lectures = EXCLUDED.total_lectures,
                    completed_lectures = EXCLUDED.completed_lectures,
                    progress_percentage = EXCLUDED.progress_percentage,
                    status = EXCLUDED.status,
                    last_accessed = CURRENT_TIMESTAMP,
                    updated_at = CURRENT_TIMESTAMP
                RETURNING 1
            )
            -- Get the final course data with progress information
            SELECT
                sc.teaching_assign_course_id AS course_id,
                sc.course_class_name AS course_name,
                c.code AS course_code,
                sc.thumbnail_url,
                sc.description,
                u.full_name AS teacher_name,
                COALESCE(scp.progress_percentage, 0) AS progress_percentage,
                sc.enrollment_date,
                COALESCE(scp.status, 'not_started') AS status
            FROM
                student_courses sc
            JOIN
                programs.table_courses c ON sc.course_id = c.id
            JOIN
                users.table_users u ON sc.teacher_id = u.id
            LEFT JOIN
                programs.table_student_course_progress scp ON sc.student_id = scp.student_id
                AND sc.teaching_assign_course_id = scp.teaching_assign_course_id
            WHERE
                sc.student_id = @student_id
            ORDER BY
                sc.start_date DESC
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
