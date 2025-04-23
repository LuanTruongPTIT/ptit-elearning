using Dapper;
using Elearning.Common.Application.Data;
using Elearning.Common.Application.Messaging;
using Elearning.Common.Domain;
using System.Data.Common;

namespace Elearning.Modules.Program.Application.Program.CreateLecture;

internal sealed class CreateLectureCommandHandler(IDbConnectionFactory dbConnectionFactory) : ICommandHandler<CreateLectureCommand, Guid>
{

  public async Task<Result<Guid>> Handle(CreateLectureCommand request, CancellationToken cancellationToken)
  {
    await using var connection = await dbConnectionFactory.OpenConnectionAsync();

    try
    {
      // Lấy course_id từ teaching_assign_course_id
      const string getCourseIdSql = """
          SELECT course_id
          FROM programs.table_teaching_assign_courses
          WHERE id = @teaching_assign_course_id;
      """;

      var courseId = await connection.QueryFirstOrDefaultAsync<Guid?>(
          getCourseIdSql,
          new { teaching_assign_course_id = request.teaching_assign_course_id }
      );

      if (courseId == null)
      {
        return Result.Failure<Guid>(new Error(
            "CreateLecture.TeachingAssignCourseNotFound",
            "Teaching assign course not found",
            ErrorType.NotFound));
      }

      // Thêm bài giảng với course_id đã lấy được và teaching_assign_course_id
      const string insertLectureSql = """
          INSERT INTO programs.table_lectures (
              id,
              course_id,
              teaching_assign_course_id,
              title,
              description,
              content_type,
              content_url,
              youtube_video_id,
              duration,
              is_published,
              created_by
          ) VALUES (
              @id,
              @course_id,
              @teaching_assign_course_id,
              @title,
              @description,
              @content_type,
              @content_url,
              @youtube_video_id,
              @duration,
              @is_published,
              @created_by
          )
          RETURNING id;
      """;

      var id = Guid.NewGuid();

      await connection.ExecuteAsync(
          insertLectureSql,
          new
          {
            id,
            course_id = courseId,
            teaching_assign_course_id = request.teaching_assign_course_id,
            request.title,
            request.description,
            request.content_type,
            request.content_url,
            request.youtube_video_id,
            request.duration,
            request.is_published,
            request.created_by
          }
      );

      return Result.Success(id);
    }
    catch (Exception ex)
    {
      Console.WriteLine(ex);
      return Result.Failure<Guid>(new Error(
    "CreateLecture.Error",
    $"Failed to create lecture: {ex.Message}",
    ErrorType.Failure));
    }
  }
}
