using System.Data.Common;
using Dapper;
using Elearning.Common.Application.Data;
using Elearning.Common.Application.Messaging;
using Elearning.Common.Domain;

namespace Elearning.Modules.Program.Application.Program.GetCoursesByProgramAndTeacher;

internal sealed class GetCoursesByProgramAndTeacherQueryHandler(
    IDbConnectionFactory dbConnectionFactory
) : IQueryHandler<GetCoursesByProgramAndTeacherQuery, List<GetCoursesByProgramAndTeacherResponse>>
{
  public async Task<Result<List<GetCoursesByProgramAndTeacherResponse>>> Handle(
      GetCoursesByProgramAndTeacherQuery request,
      CancellationToken cancellationToken)
  {
    await using DbConnection connection = await dbConnectionFactory.OpenConnectionAsync();

    const string sql = """
            SELECT DISTINCT
                c.id as course_id,
                c.name as course_name,
                c.code as course_code,
                p.id as program_id,
                p.name as program_name
            FROM programs.table_courses c 
            INNER JOIN programs.table_program_courses pc ON c.id = pc.course_id
            INNER JOIN programs.table_programs p ON pc.program_id = p.id
            INNER JOIN programs.table_teaching_assignments ta ON ta.teacher_id = @teacher_id
            WHERE p.id = @program_id 
            AND c.id = ANY(ta.subjects)
            ORDER BY c.name;
        """;

    try
    {
      var result = await connection.QueryAsync<GetCoursesByProgramAndTeacherResponse>(
          sql,
          new { request.program_id, request.teacher_id }
      );

      return Result.Success(result.ToList());
    }
    catch (Exception ex)
    {
      return Result.Failure<List<GetCoursesByProgramAndTeacherResponse>>(
          Error.Failure("GetCoursesByProgramAndTeacher", $"An error occurred: {ex.Message}")
      );
    }
  }
}