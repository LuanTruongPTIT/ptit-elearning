using System.Data.Common;
using Dapper;
using Elearning.Common.Application.Data;
using Elearning.Common.Application.Messaging;
using Elearning.Common.Domain;

namespace Elearning.Modules.Program.Application.Program.GetCourseContent;

internal sealed class GetCourseContentQueryHandler(
    IDbConnectionFactory dbConnectionFactory
) : IQueryHandler<GetCourseContentQuery, CourseDetails>
{
    public async Task<Result<CourseDetails>> Handle(
        GetCourseContentQuery request,
        CancellationToken cancellationToken)
    {
        await using DbConnection connection = await dbConnectionFactory.OpenConnectionAsync();

        try
        {
            Console.WriteLine($"GetCourseContentQueryHandler: Starting query for CourseId={request.CourseId}, StudentId={request.StudentId}");

            // 1. Lấy thông tin cơ bản về khóa học và tiến độ học tập
            var courseDetailsSql = @"
        WITH course_progress AS (
          SELECT
            slp.lecture_id,
            slp.is_completed,
            slp.last_accessed
          FROM
            programs.table_student_lecture_progress slp
          WHERE
            slp.student_id = @StudentId
        ),
        lecture_counts AS (
          SELECT
            l.teaching_assign_course_id,
            COUNT(l.id) AS total_lectures,
            SUM(CASE WHEN cp.is_completed THEN 1 ELSE 0 END) AS completed_lectures,
            MAX(cp.last_accessed) AS last_accessed
          FROM
            programs.table_lectures l
          LEFT JOIN
            course_progress cp ON l.id = cp.lecture_id
          WHERE
            l.teaching_assign_course_id = @CourseId
          GROUP BY
            l.teaching_assign_course_id
        ),
        course_progress_status AS (
          SELECT
            teaching_assign_course_id,
            CASE
              WHEN COALESCE(total_lectures, 0) = 0 THEN 'not_started'
              WHEN completed_lectures = total_lectures THEN 'completed'
              WHEN completed_lectures > 0 THEN 'in_progress'
              ELSE 'not_started'
            END AS status
          FROM lecture_counts
        )
        SELECT
          tac.id AS course_id,
          tac.course_class_name AS course_name,
          tac.description,
          tac.thumbnail_url,
          CASE
            WHEN COALESCE(lc.total_lectures, 0) = 0 THEN 0
            ELSE ROUND((COALESCE(lc.completed_lectures, 0)::float / COALESCE(lc.total_lectures, 0)::float) * 100)
          END AS progress_percent,
          COALESCE(lc.total_lectures, 0) AS total_lectures,
          COALESCE(lc.completed_lectures, 0) AS completed_lectures,
          COALESCE(cps.status, 'not_started') AS status,
          lc.last_accessed,
          tac.created_at,
          tac.updated_at,
          u.full_name AS instructor_name,
          u.avatar_url AS instructor_avatar,
          u.email AS instructor_email
        FROM
          programs.table_teaching_assign_courses tac
        LEFT JOIN
          lecture_counts lc ON tac.id = lc.teaching_assign_course_id
        LEFT JOIN
          course_progress_status cps ON tac.id = cps.teaching_assign_course_id
        LEFT JOIN
          users.table_users u ON tac.teacher_id = u.id
        WHERE
          tac.id = @CourseId
      ";

            var parameters = new { CourseId = request.CourseId, StudentId = request.StudentId };

            Console.WriteLine($"GetCourseContentQueryHandler: Executing course details SQL query");
            var courseDetails = await connection.QueryFirstOrDefaultAsync<dynamic>(courseDetailsSql, parameters);

            if (courseDetails == null)
            {
                Console.WriteLine($"GetCourseContentQueryHandler: Course not found");
                return Result.Failure<CourseDetails>(
                    new Error("GetCourseContent.NotFound", "Course not found", ErrorType.NotFound));
            }

            Console.WriteLine($"GetCourseContentQueryHandler: Course details retrieved successfully");

            // 2. Lấy danh sách lectures
            var lecturesSql = @"
        SELECT
          l.id,
          l.title,
          l.description,
          l.material_type AS content_type,
          l.content_url,
          l.youtube_video_id,
          COALESCE(slp.is_completed, false) AS is_completed,
          l.created_at,
          l.updated_at
        FROM
          programs.table_lectures l
        LEFT JOIN
          programs.table_student_lecture_progress slp ON l.id = slp.lecture_id AND slp.student_id = @StudentId
        WHERE
          l.teaching_assign_course_id = @CourseId
      ";

            Console.WriteLine($"GetCourseContentQueryHandler: Executing lectures SQL query");
            var lectures = await connection.QueryAsync<Lecture>(lecturesSql, parameters);

            Console.WriteLine($"GetCourseContentQueryHandler: Retrieved {lectures.Count()} lectures");

            // 3. Lấy resources (từ lectures với material_type là Assignment, Resource, hoặc Exam)
            var resourcesSql = @"
        SELECT
          l.id,
          l.title,
          l.material_type AS type,
          l.content_url AS url,
          l.created_at
        FROM
          programs.table_lectures l
        WHERE
          l.teaching_assign_course_id = @CourseId
          AND l.material_type IN ('Assignment', 'Resource', 'Exam')
      ";

            Console.WriteLine($"GetCourseContentQueryHandler: Executing resources SQL query");
            var resources = await connection.QueryAsync<Resource>(resourcesSql, parameters);

            Console.WriteLine($"GetCourseContentQueryHandler: Retrieved {resources.Count()} resources");

            // 4. Mock announcements data (chưa implement table announcements)
            var announcements = new List<Announcement>
      {
        new Announcement
        {
          id = Guid.NewGuid(),
          title = "Welcome to the course!",
          content = "Welcome to this course. Please check the syllabus and course materials.",
          created_at = DateTime.UtcNow.AddDays(-7).ToString("yyyy-MM-ddTHH:mm:ssZ"),
          author = courseDetails.instructor_name ?? "Instructor"
        },
        new Announcement
        {
          id = Guid.NewGuid(),
          title = "Assignment Due Date Reminder",
          content = "Don't forget to submit your assignments on time.",
          created_at = DateTime.UtcNow.AddDays(-3).ToString("yyyy-MM-ddTHH:mm:ssZ"),
          author = courseDetails.instructor_name ?? "Instructor"
        }
      };

            // 5. Tạo response
            var response = new CourseDetails
            {
                course_id = courseDetails.course_id,
                course_name = courseDetails.course_name,
                description = courseDetails.description,
                thumbnail_url = courseDetails.thumbnail_url,
                progress_percent = Convert.ToInt32(courseDetails.progress_percent),
                total_lectures = Convert.ToInt32(courseDetails.total_lectures),
                completed_lectures = Convert.ToInt32(courseDetails.completed_lectures),
                last_accessed = courseDetails.last_accessed?.ToString("yyyy-MM-ddTHH:mm:ssZ") ?? "",
                created_at = courseDetails.created_at.ToString("yyyy-MM-ddTHH:mm:ssZ"),
                updated_at = courseDetails.updated_at.ToString("yyyy-MM-ddTHH:mm:ssZ"),
                status = courseDetails.status,
                instructor = new Instructor
                {
                    name = courseDetails.instructor_name ?? "Unknown Instructor",
                    avatar = courseDetails.instructor_avatar ?? "https://randomuser.me/api/portraits/men/32.jpg",
                    email = courseDetails.instructor_email ?? ""
                },
                lectures = lectures.Select(l => new Lecture
                {
                    id = l.id,
                    title = l.title,
                    description = l.description,
                    content_type = l.content_type,
                    content_url = l.content_url,
                    youtube_video_id = l.youtube_video_id,
                    is_completed = l.is_completed,
                    created_at = l.created_at,
                    updated_at = l.updated_at
                }).ToList(),
                announcements = announcements,
                resources = resources.ToList()
            };

            Console.WriteLine($"GetCourseContentQueryHandler: Response created successfully");
            return Result.Success(response);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"GetCourseContentQueryHandler: Error occurred - {ex.Message}");
            return Result.Failure<CourseDetails>(
                new Error("GetCourseContent.Error", ex.Message, ErrorType.Failure));
        }
    }
}
