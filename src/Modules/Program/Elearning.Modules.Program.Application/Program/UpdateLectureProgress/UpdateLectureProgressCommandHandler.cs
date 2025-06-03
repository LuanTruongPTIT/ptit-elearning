using Dapper;
using Elearning.Common.Application.Data;
using Elearning.Common.Application.Messaging;
using Elearning.Common.Domain;
using System.Data.Common;

namespace Elearning.Modules.Program.Application.Program.UpdateLectureProgress;

internal sealed class UpdateLectureProgressCommandHandler(
    IDbConnectionFactory dbConnectionFactory
) : ICommandHandler<UpdateLectureProgressCommand, bool>
{
    public async Task<Result<bool>> Handle(UpdateLectureProgressCommand request, CancellationToken cancellationToken)
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
                    "UpdateLectureProgress.LectureNotFound",
                    "Lecture not found or not published",
                    ErrorType.NotFound));
            }

            // Check if student is enrolled in the course
            const string checkEnrollmentSql = """
                SELECT 1
                FROM programs.table_student_course_enrollments sce
                WHERE sce.student_id = @StudentId
                AND sce.teaching_assign_course_id = @TeachingAssignCourseId;
            """;

            var isEnrolled = await connection.QueryFirstOrDefaultAsync<int?>(
                checkEnrollmentSql,
                new
                {
                    request.StudentId,
                    TeachingAssignCourseId = lectureInfo.teaching_assign_course_id
                }
            );

            if (isEnrolled == null)
            {
                return Result.Failure<bool>(new Error(
                    "UpdateLectureProgress.NotEnrolled",
                    "Student is not enrolled in this course",
                    ErrorType.Authorization));
            }

            // Determine if lecture should be marked as completed
            bool isCompleted = request.ProgressPercentage >= 80; // 80% threshold

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
                    @WatchPosition,
                    @ProgressPercentage,
                    @IsCompleted,
                    CURRENT_TIMESTAMP,
                    CURRENT_TIMESTAMP,
                    CURRENT_TIMESTAMP
                )
                ON CONFLICT (student_id, lecture_id)
                DO UPDATE SET
                    watch_position = GREATEST(table_student_lecture_progresses.watch_position, @WatchPosition),
                    progress_percentage = GREATEST(table_student_lecture_progresses.progress_percentage, @ProgressPercentage),
                    is_completed = table_student_lecture_progresses.is_completed OR @IsCompleted,
                    last_accessed = CURRENT_TIMESTAMP,
                    updated_at = CURRENT_TIMESTAMP
                RETURNING id;
            """;

            var progressId = await connection.ExecuteScalarAsync<Guid?>(
                upsertProgressSql,
                new
                {
                    request.StudentId,
                    request.LectureId,
                    request.WatchPosition,
                    request.ProgressPercentage,
                    IsCompleted = isCompleted
                }
            );

            if (progressId == null)
            {
                return Result.Failure<bool>(new Error(
                    "UpdateLectureProgress.UpdateFailed",
                    "Failed to update lecture progress",
                    ErrorType.Failure));
            }

            // Update course progress if lecture was completed
            if (isCompleted)
            {
                await UpdateCourseProgress(connection, request.StudentId, lectureInfo.teaching_assign_course_id);
            }

            return Result.Success(true);
        }
        catch (Exception ex)
        {
            return Result.Failure<bool>(new Error(
                "UpdateLectureProgress.Error",
                $"Failed to update lecture progress: {ex.Message}",
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
