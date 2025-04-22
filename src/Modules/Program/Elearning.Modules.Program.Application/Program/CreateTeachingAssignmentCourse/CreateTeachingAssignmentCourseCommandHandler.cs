using System.Data.Common;
using Dapper;
using Elearning.Common.Application.Data;
using Elearning.Common.Application.Messaging;
using Elearning.Common.Domain;
namespace Elearning.Modules.Program.Application.Program.CreateTeachingAssignmentCourse;

internal sealed class CreateTeachingAssignmentCourseCommandHandler :
    ICommandHandler<CreateTeachingAssignmentCourseCommand, string>
{
  private readonly IDbConnectionFactory _dbConnectionFactory;

  public CreateTeachingAssignmentCourseCommandHandler(
      IDbConnectionFactory dbConnectionFactory)
  {
    _dbConnectionFactory = dbConnectionFactory;
  }

  public async Task<Result<string>> Handle(
      CreateTeachingAssignmentCourseCommand request,
      CancellationToken cancellationToken)
  {
    await using DbConnection connection = await _dbConnectionFactory.OpenConnectionAsync();
    await using var transaction = await connection.BeginTransactionAsync(cancellationToken);

    try
    {

      // Kiểm tra xem giảng viên có đang dạy lớp này với môn học này không
      var existingCourse = await CheckExistingCourse(
          connection,
          transaction,
          Guid.Parse(request.teacher_id),
          Guid.Parse(request.class_id),
          Guid.Parse(request.course_id),
          request.start_date,
          request.end_date
      );

      if (existingCourse)
      {
        return Result.Failure<string>(
            Error.Conflict(
                "DuplicateCourse",
                "You already have an active course for this class and subject during this time period"
            )
        );
      }

      var id = Guid.NewGuid();

      const string sql = """
                INSERT INTO programs.table_teaching_assign_courses(
                    id, 
                    teacher_id,
                    course_class_name,
                    description,
                    class_id,
                    course_id,
                    start_date,
                    end_date,
                    thumbnail_url,
                    status,
                    created_at,
                    updated_at
                )
                VALUES (
                    @id,
                    @teacher_id,
                    @course_class_name,
                    @description,
                    @class_id,
                    @course_id,
                    @start_date,
                    @end_date,
                    @thumbnail_url,
                    'active',
                    CURRENT_TIMESTAMP,
                    CURRENT_TIMESTAMP
                )
            """;

      await connection.ExecuteAsync(
          sql,
          new
          {
            id,
            teacher_id = Guid.Parse(request.teacher_id),
            course_class_name = request.course_name,
            request.description,
            class_id = Guid.Parse(request.class_id),
            course_id = Guid.Parse(request.course_id),
            start_date = request.start_date,
            end_date = request.end_date,
            thumbnail_url = request.thumbnail_url // Thêm trường này
          },
          transaction
      );

      await transaction.CommitAsync(cancellationToken);

      return Result.Success(id.ToString());
    }
    catch (Exception ex)
    {
      await transaction.RollbackAsync(cancellationToken);
      return Result.Failure<string>(Error.Failure("CreateTeachingAssignmentCourse", ex.Message));
    }
  }

  private async Task<bool> CheckExistingCourse(
      DbConnection connection,
      DbTransaction transaction,
      Guid teacherId,
      Guid classId,
      Guid courseId,
      DateTime startDate,
      DateTime endDate)
  {
    const string sql = """
            SELECT EXISTS (
                SELECT 1
                FROM programs.table_teaching_assign_courses
                WHERE teacher_id = @teacher_id
                AND class_id = @class_id
                AND course_id = @course_id
                AND status = 'active'
                AND (
                    (start_date <= @end_date AND end_date >= @start_date)
                    OR
                    (start_date >= @start_date AND start_date <= @end_date)
                    OR
                    (end_date >= @start_date AND end_date <= @end_date)
                )
            )
        """;

    return await connection.ExecuteScalarAsync<bool>(
        sql,
        new
        {
          teacher_id = teacherId,
          class_id = classId,
          course_id = courseId,
          start_date = startDate,
          end_date = endDate
        },
        transaction
    );
  }
}
