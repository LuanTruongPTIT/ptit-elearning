using System.Data.Common;
using Dapper;
using Elearning.Common.Application.Data;
using Elearning.Common.Application.Messaging;
using Elearning.Common.Domain;

namespace Elearning.Modules.Program.Application.Program.GetTeachingAssignCourses;

internal sealed class GetTeachingAssignCoursesQueryHandler :
    IQueryHandler<GetTeachingAssignCoursesQuery, List<GetTeachingAssignCoursesResponse>>
{
  private readonly IDbConnectionFactory _dbConnectionFactory;

  public GetTeachingAssignCoursesQueryHandler(IDbConnectionFactory dbConnectionFactory)
  {
    _dbConnectionFactory = dbConnectionFactory;
  }

  public async Task<Result<List<GetTeachingAssignCoursesResponse>>> Handle(
      GetTeachingAssignCoursesQuery request,
      CancellationToken cancellationToken)
  {
    await using DbConnection connection = await _dbConnectionFactory.OpenConnectionAsync();

    const string sql = """
            SELECT 
                tac.id,
                tac.course_class_name,
                tac.description,
                tac.class_id,
                c.class_name,
                tac.course_id,
                tc.name as course_name,
                tc.code as course_code,
                tac.start_date,
                tac.end_date,
                tac.thumbnail_url,
                tac.status,
                tac.created_at
            FROM 
                programs.table_teaching_assign_courses tac
                INNER JOIN programs.classes c ON c.id = tac.class_id
                INNER JOIN programs.table_courses tc ON tc.id = tac.course_id
            WHERE 
                tac.teacher_id = @teacher_id
            ORDER BY 
                tac.created_at DESC
        """;

    try
    {
      var result = await connection.QueryAsync<GetTeachingAssignCoursesResponse>(
          sql,
          new { teacher_id = Guid.Parse(request.teacher_id) }
      );

      return Result.Success(result.ToList());
    }
    catch (Exception ex)
    {
      return Result.Failure<List<GetTeachingAssignCoursesResponse>>(
          Error.Failure("GetTeachingAssignCourses", ex.Message)
      );
    }
  }
}