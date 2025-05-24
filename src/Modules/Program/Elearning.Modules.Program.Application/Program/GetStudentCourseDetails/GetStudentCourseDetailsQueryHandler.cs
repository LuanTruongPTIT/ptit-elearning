using System.Data.Common;
using Dapper;
using Elearning.Common.Application.Data;
using Elearning.Common.Application.Messaging;
using Elearning.Common.Domain;

namespace Elearning.Modules.Program.Application.Program.GetStudentCourseDetails;

public sealed class GetStudentCourseDetailsQueryHandler : IQueryHandler<GetStudentCourseDetailsQuery, StudentCourseDetailsResponse>
{
  private readonly IDbConnectionFactory _dbConnectionFactory;

  public GetStudentCourseDetailsQueryHandler(IDbConnectionFactory dbConnectionFactory)
  {
    _dbConnectionFactory = dbConnectionFactory;
  }

  public async Task<Result<StudentCourseDetailsResponse>> Handle(GetStudentCourseDetailsQuery request, CancellationToken cancellationToken)
  {
    try
    {
      Console.WriteLine($"GetStudentCourseDetailsQueryHandler: Handling query for courseId {request.CourseId} and studentId {request.StudentId}");

      Console.WriteLine($"GetStudentCourseDetailsQueryHandler: Creating database connection");
      await using DbConnection connection = await _dbConnectionFactory.OpenConnectionAsync();
      Console.WriteLine($"GetStudentCourseDetailsQueryHandler: Connection state: {connection.State}");

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
              WHEN total_lectures = 0 THEN 'not_started'
              WHEN completed_lectures = total_lectures THEN 'completed'
              ELSE 'in_progress'
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
          tac.updated_at
        FROM
          programs.table_teaching_assign_courses tac
        LEFT JOIN
          lecture_counts lc ON tac.id = lc.teaching_assign_course_id
        LEFT JOIN
          course_progress_status cps ON tac.id = cps.teaching_assign_course_id
        WHERE
          tac.id = @CourseId
      ";

      var parameters = new { CourseId = request.CourseId, StudentId = request.StudentId };

      Console.WriteLine($"GetStudentCourseDetailsQueryHandler: Executing course details SQL query");
      var courseDetails = await connection.QueryFirstOrDefaultAsync<dynamic>(courseDetailsSql, parameters);

      if (courseDetails == null)
      {
        Console.WriteLine($"GetStudentCourseDetailsQueryHandler: Course not found");
        return Result.Failure<StudentCourseDetailsResponse>(
            new Error("GetStudentCourseDetails.NotFound", "Course not found", ErrorType.NotFound));
      }

      Console.WriteLine($"GetStudentCourseDetailsQueryHandler: Course details retrieved successfully");

      // 2. Lấy thông tin về giảng viên
      var instructorSql = @"
        SELECT
          u.full_name AS teacher_name,
          u.avatar_url AS avatar
        FROM
          programs.table_teaching_assign_courses tac
        JOIN
          users.table_users u ON tac.teacher_id = u.id
        WHERE
          tac.id = @CourseId
      ";

      var instructor = await connection.QueryFirstOrDefaultAsync<InstructorDto>(instructorSql, new { CourseId = request.CourseId });

      // 3. Lấy danh sách các bài giảng
      var lecturesSql = @"
        SELECT
          l.id,
          l.title,
          l.description,
          l.content_type,
          l.content_url,
          COALESCE(slp.is_completed, false) AS is_completed
        FROM
          programs.table_lectures l
        LEFT JOIN
          programs.table_student_lecture_progress slp ON l.id = slp.lecture_id AND slp.student_id = @StudentId
        WHERE
          l.teaching_assign_course_id = @CourseId
      ";

      var lectures = await connection.QueryAsync<LectureDto>(lecturesSql, parameters);

      // 4. Lấy danh sách các tài liệu (lectures với content_type là Assignment, Resource, Exam)
      var resourcesSql = @"
        SELECT
          l.id,
          l.title,
          l.content_type,
          l.content_url
        FROM
          programs.table_lectures l
        WHERE
          l.teaching_assign_course_id = @CourseId
          AND l.content_type IN ('Assignment', 'Resource', 'Exam')
        ORDER BY
          l.created_at DESC
      ";

      var resources = await connection.QueryAsync<ResourceDto>(resourcesSql, parameters);

      // 5. Tạo mock data cho announcements (vì hiện tại chưa có tính năng này)
      var announcements = new List<AnnouncementDto>
      {
        new AnnouncementDto
        {
          Id = "announcement-1",
          Title = "New Content Added",
          Content = "We've added new lectures. Check out the latest lectures!",
          Date = DateTime.UtcNow.AddDays(-5)
        },
        new AnnouncementDto
        {
          Id = "announcement-2",
          Title = "Live Q&A Session",
          Content = "Join us for a live Q&A session to get your questions answered.",
          Date = DateTime.UtcNow.AddDays(-10)
        }
      };

      // 6. Tạo response
      var response = new StudentCourseDetailsResponse
      {
        CourseId = courseDetails.course_id,
        CourseName = courseDetails.course_name,
        Description = courseDetails.description,
        ThumbnailUrl = courseDetails.thumbnail_url,
        ProgressPercent = Convert.ToDouble(courseDetails.progress_percent),
        TotalLectures = Convert.ToInt32(courseDetails.total_lectures),
        CompletedLectures = Convert.ToInt32(courseDetails.completed_lectures),
        LastAccessed = courseDetails.last_accessed,
        CreatedAt = courseDetails.created_at,
        UpdatedAt = courseDetails.updated_at,
        Status = courseDetails.status,
        Instructor = instructor ?? new InstructorDto
        {
          TeacherName = "Unknown Instructor",
          Avatar = "https://randomuser.me/api/portraits/men/32.jpg"
        },
        Lectures = lectures.ToList(),
        Announcements = announcements,
        Resources = resources.ToList()
      };

      Console.WriteLine($"GetStudentCourseDetailsQueryHandler: Found {response.Lectures.Count} lectures");
      return Result.Success(response);
    }
    catch (Exception ex)
    {
      // Log the exception
      Console.WriteLine($"Error in GetStudentCourseDetailsQueryHandler: {ex.Message}");
      Console.WriteLine($"Stack trace: {ex.StackTrace}");

      // Return failure result
      return Result.Failure<StudentCourseDetailsResponse>(
          new Error("GetStudentCourseDetails.Error", $"An error occurred while retrieving course details: {ex.Message}", ErrorType.Failure));
    }
  }
}
