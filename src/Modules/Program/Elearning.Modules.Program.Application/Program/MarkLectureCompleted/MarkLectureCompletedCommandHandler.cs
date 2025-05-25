using Dapper;
using Elearning.Common.Application.Data;
using Elearning.Common.Application.Messaging;
using Elearning.Common.Domain;
using System.Data.Common;

namespace Elearning.Modules.Program.Application.Program.MarkLectureCompleted;

internal sealed class MarkLectureCompletedCommandHandler(
    IDbConnectionFactory dbConnectionFactory
) : ICommandHandler<MarkLectureCompletedCommand, bool>
{
    public async Task<Result<bool>> Handle(MarkLectureCompletedCommand request, CancellationToken cancellationToken)
    {
        await using DbConnection connection = await dbConnectionFactory.OpenConnectionAsync();

        try
        {
            // First, check if the lecture exists and get course info
            const string checkLectureSql = """
                SELECT
                    l.id,
                    l.teaching_assign_course_id,
                    l.title
                FROM programs.table_lectures l
                WHERE l.id = @LectureId AND l.is_published = true;
            """;

            var lectureInfo = await connection.QueryFirstOrDefaultAsync(
                checkLectureSql,
                new { request.LectureId }
            );

            if (lectureInfo == null)
            {
                return Result.Failure<bool>(new Error(
                    "MarkLectureCompleted.LectureNotFound",
                    "Lecture not found or not published",
                    ErrorType.NotFound));
            }

            // Check if student is enrolled in the course
            // const string checkEnrollmentSql = """
            //     SELECT 1
            //     FROM programs.table_student_course_enrollments sce
            //     WHERE sce.student_id = @StudentId
            //     AND sce.teaching_assign_course_id = @TeachingAssignCourseId;
            // """;

            // var isEnrolled = await connection.QueryFirstOrDefaultAsync<int?>(
            //     checkEnrollmentSql,
            //     new
            //     {
            //         request.StudentId,
            //         TeachingAssignCourseId = lectureInfo.teaching_assign_course_id
            //     }
            // );

            // if (isEnrolled == null)
            // {
            //     return Result.Failure<bool>(new Error(
            //         "MarkLectureCompleted.NotEnrolled",
            //         "Student is not enrolled in this course",
            //         ErrorType.Authorization));
            // }

            // Insert or update lecture progress
            const string upsertProgressSql = """
                INSERT INTO programs.table_student_lecture_progress (
                    id,
                    student_id,
                    lecture_id,
                    watch_position,
                    progress_percentage,
                    is_completed,
                    last_accessed,
                    created_at,
                    updated_at
                ) VALUES (
                    gen_random_uuid(),
                    @StudentId,
                    @LectureId,
                    0, -- Full video watched
                    100, -- 100% completed
                    true,
                    CURRENT_TIMESTAMP,
                    CURRENT_TIMESTAMP,
                    CURRENT_TIMESTAMP
                )
                ON CONFLICT (student_id, lecture_id)
                DO UPDATE SET
                    progress_percentage = 100,
                    is_completed = true,
                    last_accessed = CURRENT_TIMESTAMP,
                    updated_at = CURRENT_TIMESTAMP
                RETURNING id;
            """;

            var progressId = await connection.ExecuteScalarAsync<Guid?>(
                upsertProgressSql,
                new
                {
                    request.StudentId,
                    request.LectureId
                }
            );

            if (progressId == null)
            {
                return Result.Failure<bool>(new Error(
                    "MarkLectureCompleted.UpdateFailed",
                    "Failed to update lecture progress",
                    ErrorType.Failure));
            }

            // Update course progress
            await UpdateCourseProgress(connection, request.StudentId, lectureInfo.teaching_assign_course_id);

            return Result.Success(true);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            return Result.Failure<bool>(new Error(
                "MarkLectureCompleted.Error",
                $"Failed to mark lecture as completed: {ex.Message}",
                ErrorType.Failure));
        }
    }

    private async Task UpdateCourseProgress(DbConnection connection, Guid studentId, Guid teachingAssignCourseId)
    {
        const string updateCourseProgressSql = """
            WITH lecture_stats AS (
                SELECT
                    COUNT(*) as total_lectures,
                    COUNT(CASE WHEN slp.is_completed = true THEN 1 END) as completed_lectures
                FROM programs.table_lectures l
                LEFT JOIN programs.table_student_lecture_progress slp
                    ON l.id = slp.lecture_id AND slp.student_id = @StudentId
                WHERE l.teaching_assign_course_id = @TeachingAssignCourseId
                AND l.is_published = true
            )
            INSERT INTO programs.table_student_course_progress (
                student_id,
                teaching_assign_course_id,
                total_lectures,
                completed_lectures,
                progress_percentage,
                status,
                last_accessed,
                created_at,
                updated_at
            )
            SELECT
                @StudentId,
                @TeachingAssignCourseId,
                ls.total_lectures,
                ls.completed_lectures,
                CASE
                    WHEN ls.total_lectures = 0 THEN 0
                    ELSE ROUND((ls.completed_lectures::float / ls.total_lectures::float) * 100)
                END,
                CASE
                    WHEN ls.total_lectures = 0 THEN 'not_started'
                    WHEN ls.completed_lectures = ls.total_lectures THEN 'completed'
                    ELSE 'in_progress'
                END,
                CURRENT_TIMESTAMP,
                CURRENT_TIMESTAMP,
                CURRENT_TIMESTAMP
            FROM lecture_stats ls
            ON CONFLICT (student_id, teaching_assign_course_id)
            DO UPDATE SET
                total_lectures = EXCLUDED.total_lectures,
                completed_lectures = EXCLUDED.completed_lectures,
                progress_percentage = EXCLUDED.progress_percentage,
                status = EXCLUDED.status,
                last_accessed = CURRENT_TIMESTAMP,
                updated_at = CURRENT_TIMESTAMP;
        """;

        await connection.ExecuteAsync(
            updateCourseProgressSql,
            new
            {
                StudentId = studentId,
                TeachingAssignCourseId = teachingAssignCourseId
            }
        );
    }
}
