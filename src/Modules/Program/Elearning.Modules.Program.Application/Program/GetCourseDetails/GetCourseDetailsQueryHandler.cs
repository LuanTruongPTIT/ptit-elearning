using System.Data.Common;
using Dapper;
using Elearning.Common.Application.Data;
using Elearning.Common.Application.Messaging;
using Elearning.Common.Domain;

namespace Elearning.Modules.Program.Application.Program.GetCourseDetails;

public sealed class GetCourseDetailsQueryHandler : IQueryHandler<GetCourseDetailsQuery, GetCourseDetailsResponse>
{
  private readonly IDbConnectionFactory _dbConnectionFactory;

  public GetCourseDetailsQueryHandler(IDbConnectionFactory dbConnectionFactory)
  {
    _dbConnectionFactory = dbConnectionFactory;
  }

  public async Task<Result<GetCourseDetailsResponse>> Handle(GetCourseDetailsQuery request, CancellationToken cancellationToken)
  {
    try
    {
      Console.WriteLine($"GetCourseDetailsQueryHandler: Handling query for courseId {request.course_id} and studentId {request.student_id}");

      Console.WriteLine($"GetCourseDetailsQueryHandler: Creating database connection");
      await using DbConnection connection = await _dbConnectionFactory.OpenConnectionAsync();
      Console.WriteLine($"GetCourseDetailsQueryHandler: Connection state: {connection.State}");

      // 1. Lấy thông tin cơ bản về khóa học
      var courseDetailsSql = @"
        WITH course_progress AS (
          SELECT
            slp.lecture_id,
            slp.is_completed
          FROM
            enrollment.table_student_lecture_progresses slp
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
          WHERE
            l.teaching_assign_course_id = @course_id
          GROUP BY
            l.teaching_assign_course_id
        )
        SELECT
          tac.id,
          tac.course_class_name AS title,
          tac.description,
          tac.thumbnail_url,
          tac.syllabus,
          COALESCE(lc.total_lectures, 0) AS total_lectures,
          COALESCE(lc.completed_lectures, 0) AS completed_lectures,
          CASE
            WHEN COALESCE(lc.total_lectures, 0) = 0 THEN 0
            ELSE ROUND((COALESCE(lc.completed_lectures, 0)::float / COALESCE(lc.total_lectures, 0)::float) * 100)
          END AS progress,
          u.full_name AS teacher_name,
          u.avatar_url AS teacher_avatar
        FROM
          programs.table_teaching_assign_courses tac
        LEFT JOIN
          lecture_counts lc ON tac.id = lc.teaching_assign_course_id
        LEFT JOIN
          users.table_users u ON tac.teacher_id = u.id
        WHERE
          tac.id = @course_id
      ";

      var parameters = new { course_id = request.course_id, student_id = request.student_id };

      Console.WriteLine($"GetCourseDetailsQueryHandler: Executing course details SQL query");
      var courseDetails = await connection.QueryFirstOrDefaultAsync<GetCourseDetailsResponse>(courseDetailsSql, parameters);

      if (courseDetails == null)
      {
        Console.WriteLine($"GetCourseDetailsQueryHandler: Course not found");
        return Result.Failure<GetCourseDetailsResponse>(
            new Error("GetCourseDetails.NotFound", "Course not found", ErrorType.NotFound));
      }

      Console.WriteLine($"GetCourseDetailsQueryHandler: Course details retrieved successfully");

      // 2. Lấy danh sách các section và lecture
      var sectionsSql = @"
        SELECT
          s.id,
          s.title,
          s.order_index
        FROM
          content_management.table_sections s
        WHERE
          s.teaching_assign_course_id = @course_id
        ORDER BY
          s.order_index
      ";

      var sections = await connection.QueryAsync<CourseSectionDto>(sectionsSql, new { course_id = request.course_id });
      courseDetails.Sections = sections.ToList();

      // 3. Lấy danh sách các bài giảng cho mỗi section
      foreach (var section in courseDetails.Sections)
      {
        var lecturesSql = @"
          SELECT
            l.id,
            l.title,
            l.description,
            l.duration,
            l.type,
            l.content_url,
            COALESCE(slp.is_completed, false) AS is_completed
          FROM
            content_management.table_lectures l
          LEFT JOIN
            enrollment.table_student_lecture_progresses slp ON l.id = slp.lecture_id AND slp.student_id = @student_id
          WHERE
            l.section_id = @SectionId
          ORDER BY
            l.order_index
        ";

        var lectures = await connection.QueryAsync<LectureDto>(lecturesSql, new { SectionId = section.Id, student_id = request.student_id });
        section.Lectures = lectures.ToList();
      }

      Console.WriteLine($"GetCourseDetailsQueryHandler: Found {courseDetails.Sections.Count} sections with total lectures: {courseDetails.TotalLectures}");
      return Result.Success(courseDetails);
    }
    catch (Exception ex)
    {
      // Log the exception
      Console.WriteLine($"Error in GetCourseDetailsQueryHandler: {ex.Message}");
      Console.WriteLine($"Stack trace: {ex.StackTrace}");

      // Return failure result
      return Result.Failure<GetCourseDetailsResponse>(
          new Error("GetCourseDetails.Error", $"An error occurred while retrieving course details: {ex.Message}", ErrorType.Failure));
    }
  }
}